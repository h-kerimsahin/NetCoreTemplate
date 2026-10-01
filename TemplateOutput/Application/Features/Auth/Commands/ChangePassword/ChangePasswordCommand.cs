using MediatR;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<ApiResponse<bool>>;