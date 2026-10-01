using MediatR;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.UserManagement.Commands.LogoutAllDevices;

public record LogoutAllDevicesCommand(Guid UserId) : IRequest<ApiResponse<bool>>;