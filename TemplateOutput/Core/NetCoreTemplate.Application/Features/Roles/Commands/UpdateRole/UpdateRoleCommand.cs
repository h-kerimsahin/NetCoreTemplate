using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Roles.Commands.UpdateRole;

public record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description,
    List<Guid>? PermissionIds
) : IRequest<ApiResponse<RoleDto>>;