using MediatR;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand(
    Guid UserId,
    Guid NotificationId
) : IRequest<ApiResponse<bool>>;