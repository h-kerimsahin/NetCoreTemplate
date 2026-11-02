using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Roles.Commands.CreateRole;

public record CreateRoleCommand(
    string Name,
    string? Description,
    List<Guid>? PermissionIds
) : IRequest<ApiResponse<RoleDto>>;