using MediatR;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead;

public record MarkAllNotificationsAsReadCommand(Guid UserId) : IRequest<ApiResponse<bool>>;