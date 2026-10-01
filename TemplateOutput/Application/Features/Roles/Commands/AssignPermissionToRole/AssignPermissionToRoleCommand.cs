using MediatR;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Roles.Commands.AssignPermissionToRole;

public record AssignPermissionToRoleCommand(
    Guid RoleId,
    List<Guid> PermissionIds
) : IRequest<ApiResponse<bool>>;