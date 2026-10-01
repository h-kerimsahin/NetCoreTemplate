using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Roles.Commands.AssignPermissionToRole;

public record AssignPermissionToRoleCommand(
    Guid RoleId,
    List<Guid> PermissionIds
) : IRequest<ApiResponse<bool>>;