using MediatR;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(
    Guid UserId,
    string? Token = null
) : IRequest<ApiResponse<bool>>;