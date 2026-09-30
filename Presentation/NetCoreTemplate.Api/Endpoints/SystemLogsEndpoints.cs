using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Api.Endpoints;

public class SystemLogsEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/system-logs").WithTags("SystemLogs").RequireAuthorization();

        group.MapGet("", async ([FromQuery] SystemLogLevel? level, [FromServices] IUnitOfWork uow, [FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 20) =>
        {
            var query = level.HasValue
                ? uow.SystemLogs.GetWhere(l => l.LogLevel == level.Value, false)
                : uow.SystemLogs.GetAll(false);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(l => l.CreatedDate)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var pagedResponse = ApiResponse.Paged<SystemLog>(items, PageNumber, PageSize, totalCount, StatusCodes.Status200OK, "Sistem logları getirildi.");
            return Results.Json(pagedResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: pagedResponse.StatusCode);
        }).WithName("GetSystemLogs");

        group.MapGet("activities", async ([FromQuery] UserActivityType? type, [FromServices] IUnitOfWork uow, [FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 20) =>
        {
            var query = type.HasValue
                ? uow.AppUserActivities.GetWhere(a => a.ActivityType == type.Value, false)
                : uow.AppUserActivities.GetAll(false);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(a => a.CreatedDate)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var pagedResponse = ApiResponse.Paged<AppUserActivited>(items, PageNumber, PageSize, totalCount, StatusCodes.Status200OK, "Kullanıcı aktiviteleri getirildi.");
            return Results.Json(pagedResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: pagedResponse.StatusCode);
        }).WithName("GetUserActivities");
    }
}
