using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Api.Idempotency;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Interfaces;

namespace $safeprojectname$.Api.Endpoints;

public class SoftRestoreEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/soft-restore")
            .WithTags("SoftRestore")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,SuperAdmin" });

        group.MapGet("deleted/{entityTypeName}", async ([FromRoute] string entityTypeName, [FromServices] IUnitOfWork uow, [FromQuery] int page = 1, [FromQuery] int size = 20) =>
        {
            var list = new List<object>();
            long totalCount = 0;
            var type = entityTypeName.ToLower();

            if (type == "appuser" || type == "user")
            {
                var q = uow.AppUsers.GetAll(true).IgnoreQueryFilters().Where(e => e.Status == Domain.Enums.EntityStatus.Deleted);
                totalCount = await q.CountAsync();
                list = await q.Skip((page - 1) * size).Take(size).Cast<object>().ToListAsync();
            }
            else if (type == "approle" || type == "role")
            {
                var q = uow.AppRoles.GetAll(true).IgnoreQueryFilters().Where(e => e.Status == Domain.Enums.EntityStatus.Deleted);
                totalCount = await q.CountAsync();
                list = await q.Skip((page - 1) * size).Take(size).Cast<object>().ToListAsync();
            }

            var paged = ApiResponse.Paged(list, page, size, totalCount);
            return Results.Json(paged, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: paged.StatusCode);
        }).WithName("GetDeletedEntitiesPaged");

        group.MapPost("restore", async ([FromBody] RestoreEntityRequestDto req, [FromServices] IUnitOfWork uow) =>
        {
            var type = req.EntityTypeName.ToLower();
            if (type == "appuser" || type == "user")
            {
                var entity = await uow.AppUsers.GetAll(true).IgnoreQueryFilters().Where(e => e.Id == req.EntityId).FirstOrDefaultAsync();
                if (entity != null)
                {
                    entity.Restore();
                    await uow.SaveChangesAsync();
                }
            }
            else if (type == "approle" || type == "role")
            {
                var entity = await uow.AppRoles.GetAll(true).IgnoreQueryFilters().Where(e => e.Id == req.EntityId).FirstOrDefaultAsync();
                if (entity != null)
                {
                    entity.Restore();
                    await uow.SaveChangesAsync();
                }
            }

            var resp = ApiResponse.Success(StatusCodes.Status200OK, "Veri geri yüklendi.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).WithName("RestoreDeletedEntity").AddEndpointFilter<IdempotencyEndpointFilter>();
    }
}
