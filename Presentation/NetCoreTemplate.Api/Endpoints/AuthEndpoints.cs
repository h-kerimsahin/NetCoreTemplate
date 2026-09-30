using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetCoreTemplate.Api.Idempotency;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Application.Features.Auth.Commands.EnableTwoFactor;
using NetCoreTemplate.Application.Features.Auth.Commands.Login;
using NetCoreTemplate.Application.Features.Auth.Commands.Logout;
using NetCoreTemplate.Application.Features.Auth.Commands.RefreshToken;
using NetCoreTemplate.Application.Features.Auth.Commands.Register;
using NetCoreTemplate.Application.Features.Auth.Commands.ResetPassword;
using NetCoreTemplate.Application.Features.Auth.Commands.ForgotPassword;
using NetCoreTemplate.Application.Features.Auth.Commands.VerifyTwoFactor;
using NetCoreTemplate.Application.Features.Auth.Queries.Me;
using NetCoreTemplate.Domain.Enums;

namespace NetCoreTemplate.Api.Endpoints;

public class AuthEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth").WithTags("Auth");

        group.MapPost("login", async ([FromBody] LoginRequestDto req, [FromServices] ISender sender, HttpContext ctx) =>
        {
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var ua = ctx.Request.Headers["User-Agent"].ToString();
            var response = await sender.Send(new LoginCommand(req.EmailOrUserName, req.Password, req.RememberMe, ip, ua));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).AllowAnonymous().WithName("Login").AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPost("register", async ([FromBody] RegisterRequestDto req, [FromServices] ISender sender, HttpContext ctx) =>
        {
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var response = await sender.Send(new RegisterCommand(req.UserName, req.Email, req.Password, req.ConfirmPassword, req.FirstName, req.LastName, ip));
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).AllowAnonymous().WithName("Register").AddEndpointFilter<IdempotencyEndpointFilter>();

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
