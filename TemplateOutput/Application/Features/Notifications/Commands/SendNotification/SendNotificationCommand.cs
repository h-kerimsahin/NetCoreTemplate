using MediatR;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Enums;

namespace $safeprojectname$.Application.Features.Notifications.Commands.SendNotification;

public record SendNotificationCommand(
    Guid? UserId,
    string Title,
    string Message,
    NotificationType Type,
    object? Data = null
) : IRequest<ApiResponse<bool>>;