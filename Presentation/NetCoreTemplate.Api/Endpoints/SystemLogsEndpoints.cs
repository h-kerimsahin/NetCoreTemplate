using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Api.Endpoints;

public class SystemLogsEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/system-logs")
            .WithTags("SystemLogs")
            .RequireAuthorization();

        group.MapGet("", async ([FromQuery] SystemLogLevel? level, [FromServices] IUnitOfWork uow, [FromQuery] int page = 1, [FromQuery] int size = 20) =>
        {
            var query = level.HasValue
                ? uow.SystemLogs.GetWhere(l => l.LogLevel == level.Value, false)
                : uow.SystemLogs.GetAll(false);

            long totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.CreatedDate)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var pagedResponse = ApiResponse.Paged<SystemLog>(items, page, size, totalCount, StatusCodes.Status200OK, "Sistem logları getirildi.");
            return Results.Json(pagedResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: pagedResponse.StatusCode);
        }).WithName("GetSystemLogs");

        group.MapGet("activities", async ([FromQuery] UserActivityType? type, [FromServices] IUnitOfWork uow, [FromQuery] int page = 1, [FromQuery] int size = 20) =>
        {
            var query = type.HasValue
                ? uow.AppUserActivities.GetWhere(a => a.ActivityType == type.Value, false)
                : uow.AppUserActivities.GetAll(false);

            long totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.CreatedDate)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var pagedResponse = ApiResponse.Paged<AppUserActivited>(items, page, size, totalCount, StatusCodes.Status200OK, "Kullanıcı aktiviteleri getirildi.");
            return Results.Json(pagedResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: pagedResponse.StatusCode);
        }).WithName("GetUserActivities");
    }
}
