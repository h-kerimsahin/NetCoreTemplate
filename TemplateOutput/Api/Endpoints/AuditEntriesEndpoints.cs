using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Interfaces;

namespace $safeprojectname$.Api.Endpoints;

public class AuditEntriesEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/audit-entries")
            .WithTags("AuditEntries")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,SuperAdmin" });

        group.MapGet("entity/{entityName}/{entityId}", async ([FromRoute] string entityName, [FromRoute] Guid entityId, [FromServices] IUnitOfWork uow, [FromQuery] int page = 1, [FromQuery] int size = 20) =>
        {
            var query = uow.AuditEntries.GetWhere(a => a.EntityName == entityName && a.EntityId == entityId, false);
            long totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(a => a.ChangedAt).Skip((page - 1) * size).Take(size).ToListAsync();
            var paged = ApiResponse.Paged(items, page, size, totalCount);
            return Results.Json(paged, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: paged.StatusCode);
        }).WithName("GetAuditEntriesByEntity");

        group.MapGet("user/{changedByUserId}", async ([FromRoute] Guid changedByUserId, [FromServices] IUnitOfWork uow, [FromQuery] int page = 1, [FromQuery] int size = 20) =>
        {
            var query = uow.AuditEntries.GetWhere(a => a.ChangedByUserId == changedByUserId, false);
            long totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(a => a.ChangedAt).Skip((page - 1) * size).Take(size).ToListAsync();
            var paged = ApiResponse.Paged(items, page, size, totalCount);
            return Results.Json(paged, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: paged.StatusCode);
        }).WithName("GetAuditEntriesByUser");
    }
}
