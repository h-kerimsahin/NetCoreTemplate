using MediatR;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.UserManagement.Commands.UnlockUser;

public record UnlockUserCommand(Guid UserId) : IRequest<ApiResponse<bool>>;