using MediatR;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;

namespace NetCoreTemplate.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string? IpAddress = null) : IRequest<ApiResponse<RefreshTokenResponseDto>>;
