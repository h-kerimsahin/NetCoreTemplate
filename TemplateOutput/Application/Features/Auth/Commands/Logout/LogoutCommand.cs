using MediatR;
using $safeprojectname$.Application.DTOs.Auth;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(Guid UserId, string? IpAddress = null) : IRequest<ApiResponse<LogoutResponse>>;
