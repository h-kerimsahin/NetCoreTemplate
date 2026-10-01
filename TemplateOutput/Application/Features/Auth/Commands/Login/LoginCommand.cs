using MediatR;
using $safeprojectname$.Application.DTOs.Auth;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Auth.Commands.Login;

public record LoginCommand(string EmailOrUserName, string Password, bool RememberMe, string? IpAddress = null, string? UserAgent = null) : IRequest<ApiResponse<LoginResponseDto>>;
