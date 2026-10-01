using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.UserManagement.Commands.RevokeRoleFromUser;

public record RevokeRoleFromUserCommand(
    Guid UserId,
    Guid RoleId
) : IRequest<ApiResponse<bool>>;