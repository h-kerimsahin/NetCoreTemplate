using MediatR;
using NetCoreTemplate.Application.DTOs.Auth;

namespace NetCoreTemplate.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string? IpAddress = null) : IRequest<RefreshTokenResponseDto>;
