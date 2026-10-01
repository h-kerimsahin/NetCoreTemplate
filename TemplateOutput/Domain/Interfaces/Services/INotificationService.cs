using $safeprojectname$.Domain.Enums;

namespace $safeprojectname$.Domain.Interfaces.Services;

public interface INotificationService
{
    Task SendToUserAsync(Guid userId, string title, string message, NotificationType type, object? data = null, CancellationToken cancellationToken = default);
    Task SendToRoleAsync(Guid roleId, string title, string message, NotificationType type, object? data = null, CancellationToken cancellationToken = default);
    Task SendToAllAsync(string title, string message, NotificationType type, object? data = null, CancellationToken cancellationToken = default);
    Task SendToConnectionAsync(string connId, string title, string message, NotificationType type, object? data = null, CancellationToken cancellationToken = default);
}
