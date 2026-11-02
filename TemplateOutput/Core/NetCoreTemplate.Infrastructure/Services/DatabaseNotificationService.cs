using System.Text.Json;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;
using NetCoreTemplate.Domain.Interfaces.Services;

namespace NetCoreTemplate.Infrastructure.Services;

public class DatabaseNotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _userActivityLogger;

    public DatabaseNotificationService(IUnitOfWork unitOfWork, IUserActivityLogger userActivityLogger)
    {
        _unitOfWork = unitOfWork;
        _userActivityLogger = userActivityLogger;
    }

    public async Task SendToUserAsync(Guid userId, string title, string message, NotificationType type, object? data = null, CancellationToken cancellationToken = default)
    {
        var jsonData = data != null ? JsonSerializer.Serialize(data) : null;
        var notification = AppNotification.Create(userId, title, message, type, jsonData);

        await _unitOfWork.AppNotifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _userActivityLogger.LogAsync(userId, UserActivityType.NotificationSent,
            $"Notification sent: {title}", nameof(AppNotification), notification.Id,
            cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SendToRoleAsync(Guid roleId, string title, string message, NotificationType type, object? data = null, CancellationToken cancellationToken = default)
    {
        var userIds = await _unitOfWork.AppUsers.GetUserIdsByRoleIdAsync(roleId, cancellationToken);
        var jsonData = data != null ? JsonSerializer.Serialize(data) : null;

        foreach (var userId in userIds)
        {
            var notification = AppNotification.Create(userId, title, message, type, jsonData);
            await _unitOfWork.AppNotifications.AddAsync(notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SendToAllAsync(string title, string message, NotificationType type, object? data = null, CancellationToken cancellationToken = default)
    {
        var jsonData = data != null ? JsonSerializer.Serialize(data) : null;

        var globalNotification = AppNotification.Create(null, title, message, type, jsonData);
        await _unitOfWork.AppNotifications.AddAsync(globalNotification, cancellationToken);

        var userIds = await _unitOfWork.AppUsers.GetAllUserIdsAsync(cancellationToken);
        foreach (var userId in userIds)
        {
            var notification = AppNotification.Create(userId, title, message, type, jsonData);
            await _unitOfWork.AppNotifications.AddAsync(notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SendToConnectionAsync(string connId, string title, string message, NotificationType type, object? data = null, CancellationToken cancellationToken = default)
    {
        var userId = await _unitOfWork.AppUsers.GetUserIdByConnectionIdAsync(connId, cancellationToken);
        if (!userId.HasValue) return;

        var jsonData = data != null ? JsonSerializer.Serialize(data) : null;
        var notification = AppNotification.Create(userId.Value, title, message, type, jsonData);

        await _unitOfWork.AppNotifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _userActivityLogger.LogAsync(userId.Value, UserActivityType.NotificationSent,
            $"Notification sent (connection): {title}", nameof(AppNotification), notification.Id,
            cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
