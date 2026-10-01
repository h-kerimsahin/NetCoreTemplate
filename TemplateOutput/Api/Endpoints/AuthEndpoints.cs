using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using $safeprojectname$.Api.Idempotency;
using $safeprojectname$.Api.Middlewares;
using $safeprojectname$.Application.DTOs.Auth;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Features.Auth.Commands.EnableTwoFactor;
using $safeprojectname$.Application.Features.Auth.Commands.Login;
using $safeprojectname$.Application.Features.Auth.Commands.Logout;
using $safeprojectname$.Application.Features.Auth.Commands.RefreshToken;
using $safeprojectname$.Application.Features.Auth.Commands.Register;
using $safeprojectname$.Application.Features.Auth.Commands.ResetPassword;
using $safeprojectname$.Application.Features.Auth.Commands.ForgotPassword;
using $safeprojectname$.Application.Features.Auth.Commands.VerifyTwoFactor;
using $safeprojectname$.Application.Features.Auth.Queries.Me;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;

namespace $safeprojectname$.Api.Endpoints;

public class AuthEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/auth")
            .WithTags("Auth");

        group.MapPost("login", async ([FromBody] LoginRequestDto req, [FromServices] ISender sender, HttpContext ctx) =>
        {
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var ua = ctx.Request.Headers["User-Agent"].ToString();
            var response = await sender.Send(new LoginCommand(req.EmailOrUserName, req.Password, req.RememberMe, ip, ua));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).AllowAnonymous().WithName("Login").AddEndpointFilter<IdempotencyEndpointFilter>().RequireRateLimiting(RateLimitingPolicy.AuthFixedWindowPolicy);

        group.MapPost("register", async ([FromBody] RegisterRequestDto req, [FromServices] ISender sender, HttpContext ctx) =>
        {
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var response = await sender.Send(new RegisterCommand(req.UserName, req.Email, req.Password, req.ConfirmPassword, req.FirstName, req.LastName, ip));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).AllowAnonymous().WithName("Register").AddEndpointFilter<IdempotencyEndpointFilter>().RequireRateLimiting(RateLimitingPolicy.AuthFixedWindowPolicy);

        group.MapPost("change-password", async ([FromBody] ChangePasswordRequest req, ClaimsPrincipal user, [FromServices] IUnitOfWork uow, [FromServices] Domain.Interfaces.Security.IPasswordHasher hasher) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var usr = await uow.AppUsers.GetByIdAsync(userId);
            if (usr == null)
            {
                var nf = ApiResponse.Fail(StatusCodes.Status404NotFound, "Kullanıcı bulunamadı.");
                return Results.Json(nf, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: nf.StatusCode);
            }
            if (!hasher.VerifyPassword(usr.PasswordHash, req.CurrentPassword))
            {
                var fail = ApiResponse.Fail(StatusCodes.Status400BadRequest, "Mevcut şifre yanlış.");
                return Results.Json(fail, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: fail.StatusCode);
            }
            usr.UpdatePassword(hasher.HashPassword(req.NewPassword));
            await uow.SaveChangesAsync();
            var resp = ApiResponse.Success(StatusCodes.Status200OK, "Şifre değiştirildi.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).RequireAuthorization().WithName("ChangePassword").AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPost("confirm-email", async ([FromBody] ConfirmEmailRequest req, [FromServices] IUnitOfWork uow) =>
        {
            var user = await uow.AppUsers.GetByIdAsync(req.UserId);
            if (user == null)
            {
                var nf = ApiResponse.Fail(StatusCodes.Status404NotFound, "Kullanıcı bulunamadı.");
                return Results.Json(nf, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: nf.StatusCode);
            }
            user.ConfirmEmail();
            await uow.SaveChangesAsync();
            var resp = ApiResponse.Success(StatusCodes.Status200OK, "E-posta doğrulandı.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).AllowAnonymous().WithName("ConfirmEmail").AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPost("logout-all-devices", async (ClaimsPrincipal user, [FromServices] IUnitOfWork uow) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var usr = await uow.AppUsers.GetByIdAsync(userId);
            if (usr == null)
            {
                var nf = ApiResponse.Fail(StatusCodes.Status404NotFound, "Kullanıcı bulunamadı.");
                return Results.Json(nf, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: nf.StatusCode);
            }
            usr.RefreshSecurityStamp();
            await uow.AppUserRefreshTokens.RevokeAllTokensForUserAsync(userId);
            await uow.SaveChangesAsync();
            var resp = ApiResponse.Success(StatusCodes.Status200OK, "Tüm cihazlardan çıkış yapıldı.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).RequireAuthorization().WithName("LogoutAllDevicesAuth").AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPost("refresh", async ([FromBody] RefreshTokenRequestDto req, [FromServices] ISender sender, HttpContext ctx) =>
        {
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var response = await sender.Send(new RefreshTokenCommand(req.RefreshToken, ip));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).AllowAnonymous().AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPost("forgot-password", async ([FromBody] ForgotPasswordRequestDto req, [FromServices] ISender sender) =>
        {
            var response = await sender.Send(new ForgotPasswordCommand(req.Email));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).AllowAnonymous().AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPost("reset-password", async ([FromBody] ResetPasswordRequestDto req, [FromServices] ISender sender) =>
        {
            var response = await sender.Send(new ResetPasswordCommand(req.Email, req.Token, req.NewPassword, req.ConfirmNewPassword));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).AllowAnonymous().AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPost("enable-2fa", async ([FromBody] EnableTwoFactorRequestDto req, [FromServices] ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var response = await sender.Send(new EnableTwoFactorCommand(userId, req.Type));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).RequireAuthorization().AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPost("verify-2fa", async ([FromBody] VerifyTwoFactorRequestDto req, [FromServices] ISender sender, HttpContext ctx) =>
        {
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var ua = ctx.Request.Headers["User-Agent"].ToString();
            var response = await sender.Send(new VerifyTwoFactorCommand(req.EmailOrUserName, req.Code, ip, ua));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).AllowAnonymous().AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPost("logout", async ([FromServices] ISender sender, ClaimsPrincipal user, HttpContext ctx) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var response = await sender.Send(new LogoutCommand(userId, ip));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).RequireAuthorization().AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapGet("me", async ([FromServices] ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var response = await sender.Send(new MeQuery(userId));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).RequireAuthorization().WithName("Me");
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmNewPassword);
public record ConfirmEmailRequest(Guid UserId, string Token);
