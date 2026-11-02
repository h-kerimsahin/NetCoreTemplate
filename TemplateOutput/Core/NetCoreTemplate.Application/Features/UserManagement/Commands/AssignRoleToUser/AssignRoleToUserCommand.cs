using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.UserManagement.Commands.AssignRoleToUser;

public record AssignRoleToUserCommand(
    Guid UserId,
    List<Guid> RoleIds
) : IRequest<ApiResponse<bool>>;