using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Api.Hubs;
using $safeprojectname$.Api.Idempotency;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;

namespace $safeprojectname$.Api.Endpoints;

public class NotificationsEndpoints : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/notifications")
            .WithTags("Notifications");

        group.MapPost("send", async ([FromBody] NotificationSendRequestDto req, [FromServices] IUnitOfWork uow, [FromServices] IHubContext<NotificationHub> hubContext) =>
        {
            var notification = AppNotification.Create(req.UserId, req.Title, req.Message, (NotificationType)req.Type, req.Data);
            await uow.AppNotifications.AddAsync(notification);
            await uow.SaveChangesAsync();

            if (req.UserId.HasValue)
            {
                await hubContext.Clients.Group($"user-{req.UserId.Value}").SendAsync("ReceiveNotification", notification);
            }
            else
            {
                await hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
            }

            var resp = ApiResponse.Success(notification, StatusCodes.Status201Created, "Bildirim gönderildi.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).WithName("SendNotification").RequireAuthorization(new AuthorizeAttribute { Roles = "Admin,SuperAdmin" }).AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapGet("mine", async (ClaimsPrincipal user, [FromServices] IUnitOfWork uow, [FromQuery] int page = 1, [FromQuery] int size = 20) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var query = uow.AppNotifications.GetWhere(n => n.UserId == userId, false);
            long totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(n => n.CreatedAt).Skip((page - 1) * size).Take(size).ToListAsync();
            var paged = ApiResponse.Paged(items, page, size, totalCount);
            return Results.Json(paged, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: paged.StatusCode);
        }).WithName("GetMyNotificationsPaged").RequireAuthorization();

        group.MapPost("{notificationId}/mark-as-read", async ([FromRoute] Guid notificationId, ClaimsPrincipal user, [FromServices] IUnitOfWork uow) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notif = await uow.AppNotifications.GetWhere(n => n.Id == notificationId && n.UserId == userId, false).FirstOrDefaultAsync();
            if (notif == null)
            {
                var nf = ApiResponse.Fail(StatusCodes.Status404NotFound, "Bildirim bulunamadı.");
                return Results.Json(nf, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: nf.StatusCode);
            }
            notif.MarkAsRead();
            await uow.SaveChangesAsync();
            var resp = ApiResponse.Success(StatusCodes.Status200OK, "Bildirim okundu olarak işaretlendi.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).WithName("MarkNotificationAsRead").RequireAuthorization().AddEndpointFilter<IdempotencyEndpointFilter>();

        group.MapPost("mark-all-as-read", async (ClaimsPrincipal user, [FromServices] IUnitOfWork uow) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notifs = await uow.AppNotifications.GetWhere(n => n.UserId == userId && !n.IsRead, false).ToListAsync();
            foreach (var n in notifs) n.MarkAsRead();
            await uow.SaveChangesAsync();
            var resp = ApiResponse.Success(StatusCodes.Status200OK, $"{notifs.Count} bildirim okundu olarak işaretlendi.");
            return Results.Json(resp, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: resp.StatusCode);
        }).WithName("MarkAllNotificationsAsRead").RequireAuthorization().AddEndpointFilter<IdempotencyEndpointFilter>();
    }
}
