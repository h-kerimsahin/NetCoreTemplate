using MediatR;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.UserManagement.Commands.AssignRoleToUser;

public record AssignRoleToUserCommand(
    Guid UserId,
    List<Guid> RoleIds
) : IRequest<ApiResponse<bool>>;