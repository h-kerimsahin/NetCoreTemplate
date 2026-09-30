using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Api.Endpoints;

public class SystemLogsEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/system-logs").WithTags("SystemLogs").RequireAuthorization();

        group.MapGet("", async ([FromQuery] SystemLogLevel? level, [FromServices] IUnitOfWork uow, [FromQuery] int take = 100) =>
        {
            var logs = level.HasValue
                ? await uow.SystemLogs.GetByLogLevelAsync(level.Value, take)
                : uow.SystemLogs.GetAll(false).OrderByDescending(l => l.CreatedDate).Take(take).ToList();
            return Results.Ok(logs);
        }).WithName("GetSystemLogs");

        group.MapGet("activities", async ([FromQuery] UserActivityType? type, [FromServices] IUnitOfWork uow, [FromQuery] int take = 100) =>
        {
            var activities = type.HasValue
                ? await uow.AppUserActivities.GetByActivityTypeAsync(type.Value, take)
                : uow.AppUserActivities.GetAll(false).OrderByDescending(a => a.CreatedDate).Take(take).ToList();
            return Results.Ok(activities);
        }).WithName("GetUserActivities");
    }
}
