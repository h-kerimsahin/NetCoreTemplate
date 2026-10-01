using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Api.Middlewares;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Interfaces;

namespace $safeprojectname$.Api.Endpoints;

public class UserManagementEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/users-management")
            .WithTags("UserManagement")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,SuperAdmin" });

        group.MapPost("assign-role", async ([FromBody] RoleAssignRequestDto req, [FromServices] IUnitOfWork uow) =>
        {
            var user = await uow.AppUsers.GetByIdAsync(req.UserId);
            if (user == null)
            {
                var nf = ApiResponse.Fail(StatusCodes.Status404NotFound, "Kullanıcı bulunamadı.");
                return Results.Json(nf, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: nf.StatusCode);
            }
            var existing = uow.AppUserRoles.GetWhere(ur => ur.UserId == req.UserId, false).ToList();
            foreach (var ex in existing) uow.AppUserRoles.Delete(ex);
            foreach (var roleId in req.RoleIds)
            {
                await uow.AppUserRoles.AddAsync(new AppUserRole(req.UserId, roleId));
            }
            user.RefreshSecurityStamp();
            await uow.SaveChangesAsync();
            var resp = ApiResponse.Success(StatusCodes.Status200OK, "Roller atandı.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).WithName("AssignRoleToUser");

        group.MapPost("revoke-role", async ([FromBody] RoleAssignRequestDto req, [FromServices] IUnitOfWork uow) =>
        {
            var existing = uow.AppUserRoles.GetWhere(ur => ur.UserId == req.UserId && req.RoleIds.Contains(ur.RoleId), false).ToList();
            foreach (var ex in existing) uow.AppUserRoles.Delete(ex);
            var user = await uow.AppUsers.GetByIdAsync(req.UserId);
            if (user != null) user.RefreshSecurityStamp();
            await uow.SaveChangesAsync();
            var resp = ApiResponse.Success(StatusCodes.Status200OK, "Roller geri alındı.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).WithName("RevokeRoleFromUser");

        group.MapPost("{userId}/logout-all-devices", async ([FromRoute] Guid userId, [FromServices] IUnitOfWork uow) =>
        {
            var user = await uow.AppUsers.GetByIdAsync(userId);
            if (user == null)
            {
                var nf = ApiResponse.Fail(StatusCodes.Status404NotFound, "Kullanıcı bulunamadı.");
                return Results.Json(nf, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: nf.StatusCode);
            }
            user.RefreshSecurityStamp();
            await uow.AppUserRefreshTokens.RevokeAllTokensForUserAsync(userId);
            await uow.SaveChangesAsync();
            var resp = ApiResponse.Success(StatusCodes.Status200OK, "Tüm cihazlardan çıkış yapıldı.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).WithName("LogoutAllDevices");

        group.MapPost("{userId}/unlock", async ([FromRoute] Guid userId, [FromServices] IUnitOfWork uow) =>
        {
            var user = await uow.AppUsers.GetByIdAsync(userId);
            if (user == null)
            {
                var nf = ApiResponse.Fail(StatusCodes.Status404NotFound, "Kullanıcı bulunamadı.");
                return Results.Json(nf, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: nf.StatusCode);
            }
            user.Unlock();
            await uow.SaveChangesAsync();
            var resp = ApiResponse.Success(StatusCodes.Status200OK, "Kullanıcı kilidi açıldı.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).WithName("UnlockUser");

        group.MapGet("paged", async ([FromServices] IUnitOfWork uow, [FromQuery] int page = 1, [FromQuery] int size = 20) =>
        {
            var query = uow.AppUsers.GetAll(false);
            long totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            var paged = ApiResponse.Paged(items, page, size, totalCount);
            return Results.Json(paged, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: paged.StatusCode);
        }).WithName("GetAllUsersPaged");
    }
}
