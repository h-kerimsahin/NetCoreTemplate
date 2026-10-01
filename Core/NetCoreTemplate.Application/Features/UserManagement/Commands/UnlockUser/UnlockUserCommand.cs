using MediatR;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.UserManagement.Commands.UnlockUser;

public record UnlockUserCommand(Guid UserId) : IRequest<ApiResponse<bool>>;