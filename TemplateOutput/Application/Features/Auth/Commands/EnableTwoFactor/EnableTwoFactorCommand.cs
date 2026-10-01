using MediatR;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Auth.Commands.EnableTwoFactor;

public record EnableTwoFactorCommand(Guid UserId, string Type = "Email") : IRequest<ApiResponse<bool>>;
