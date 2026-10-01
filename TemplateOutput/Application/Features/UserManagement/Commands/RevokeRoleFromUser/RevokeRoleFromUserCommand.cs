using MediatR;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.UserManagement.Commands.RevokeRoleFromUser;

public record RevokeRoleFromUserCommand(
    Guid UserId,
    Guid RoleId
) : IRequest<ApiResponse<bool>>;