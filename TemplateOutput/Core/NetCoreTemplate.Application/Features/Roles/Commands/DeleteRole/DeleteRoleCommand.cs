using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(Guid Id) : IRequest<ApiResponse<bool>>;