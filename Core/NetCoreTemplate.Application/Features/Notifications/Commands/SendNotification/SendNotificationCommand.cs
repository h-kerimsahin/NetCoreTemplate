using MediatR;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Domain.Enums;

namespace NetCoreTemplate.Application.Features.Notifications.Commands.SendNotification;

public record SendNotificationCommand(
    Guid? UserId,
    string Title,
    string Message,
    NotificationType Type,
    object? Data = null
) : IRequest<ApiResponse<bool>>;