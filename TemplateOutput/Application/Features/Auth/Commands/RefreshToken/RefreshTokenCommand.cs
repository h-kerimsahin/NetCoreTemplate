using MediatR;
using $safeprojectname$.Application.DTOs.Auth;
using $safeprojectname$.Application.DTOs.Common;

namespace $safeprojectname$.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string? IpAddress = null) : IRequest<ApiResponse<RefreshTokenResponseDto>>;
