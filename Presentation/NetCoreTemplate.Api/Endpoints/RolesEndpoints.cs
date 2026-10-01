using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Api.Idempotency;
using NetCoreTemplate.Api.Middlewares;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Api.Endpoints;

public class RolesEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/roles")
            .WithTags("Roles")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,SuperAdmin" });

        group.MapPost("", async ([FromBody] RoleCreateRequest req, [FromServices] ISender sender, [FromServices] IUnitOfWork uow, ClaimsPrincipal user, HttpContext ctx) =>
        {
            var role = AppRole.Create(req.Name, req.Description);
            await uow.AppRoles.AddAsync(role);
            await uow.SaveChangesAsync();
            var response = ApiResponse.Success(role, StatusCodes.Status201Created, "Rol oluşturuldu.");
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).WithName("CreateRole").AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPut("{id:guid}", async ([FromRoute] Guid id, [FromBody] RoleUpdateRequest req, [FromServices] IUnitOfWork uow) =>
        {
            var role = await uow.AppRoles.GetByIdAsync(id);
            if (role == null)
            {
                var notFound = ApiResponse.Fail(StatusCodes.Status404NotFound, "Rol bulunamadı.");
                return Results.Json(notFound, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: notFound.StatusCode);
            }
            role.UpdateDetails(req.Name, req.Description);
            await uow.SaveChangesAsync();
            var response = ApiResponse.Success(role, StatusCodes.Status200OK, "Rol güncellendi.");
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).WithName("UpdateRole").AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapDelete("{id:guid}", async ([FromRoute] Guid id, [FromServices] IUnitOfWork uow) =>
        {
            var role = await uow.AppRoles.GetByIdAsync(id);
            if (role == null)
            {
                var notFound = ApiResponse.Fail(StatusCodes.Status404NotFound, "Rol bulunamadı.");
                return Results.Json(notFound, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: notFound.StatusCode);
            }
            uow.AppRoles.Delete(role);
            await uow.SaveChangesAsync();
            var response = ApiResponse.Success(StatusCodes.Status200OK, "Rol silindi.");
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).WithName("DeleteRole");

        group.MapPost("{roleId}/permissions", async ([FromRoute] Guid roleId, [FromBody] PermissionGrantDto req, [FromServices] IUnitOfWork uow) =>
        {
            var role = await uow.AppRoles.GetByIdAsync(roleId);
            if (role == null)
            {
                var notFound = ApiResponse.Fail(StatusCodes.Status404NotFound, "Rol bulunamadı.");
                return Results.Json(notFound, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: notFound.StatusCode);
            }
            var existing = uow.AppRolePermissions.GetWhere(rp => rp.RoleId == roleId, false).ToList();
            foreach (var ex in existing) uow.AppRolePermissions.Delete(ex);
            foreach (var permId in req.PermissionIds)
            {
                await uow.AppRolePermissions.AddAsync(new AppRolePermission(roleId, permId));
            }
            await uow.SaveChangesAsync();
            var response = ApiResponse.Success(StatusCodes.Status200OK, "İzinler atandı.");
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).WithName("AssignPermission").AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapGet("{id:guid}", async ([FromRoute] Guid id, [FromServices] IUnitOfWork uow) =>
        {
            var role = await uow.AppRoles.GetByIdAsync(id);
            if (role == null)
            {
                var notFound = ApiResponse.Fail(StatusCodes.Status404NotFound, "Rol bulunamadı.");
                return Results.Json(notFound, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: notFound.StatusCode);
            }
            var response = ApiResponse.Success(role);
            return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
        }).WithName("GetRoleById");

        group.MapGet("", async ([FromServices] IUnitOfWork uow, ODataQueryOptions<AppRole> options, [FromQuery] int page = 1, [FromQuery] int size = 20) =>
        {
            var query = uow.AppRoles.GetAll(false);
            long totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            var paged = ApiResponse.Paged(items, page, size, totalCount);
            return Results.Json(paged, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: paged.StatusCode);
        }).WithName("GetAllRolesPaged");
    }
}

public record RoleCreateRequest(string Name, string? Description);
public record RoleUpdateRequest(string Name, string? Description);
