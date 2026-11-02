using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand(
    Guid UserId,
    Guid NotificationId
) : IRequest<ApiResponse<bool>>;