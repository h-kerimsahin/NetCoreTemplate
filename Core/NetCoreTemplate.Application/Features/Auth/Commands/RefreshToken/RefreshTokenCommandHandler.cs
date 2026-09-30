using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;

namespace NetCoreTemplate.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<RefreshTokenResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<RefreshTokenResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var activeTokens = await _unitOfWork.AppUserRefreshTokens.GetWhere(t => !t.IsRevoked && t.TokenType == TokenType.RefreshToken, false).ToListAsync(cancellationToken);
        var matching = activeTokens.FirstOrDefault(t => _passwordHasher.VerifyPassword(request.Token, t.Token));

        if (matching == null || matching.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token");

        var user = await _unitOfWork.AppUsers.GetByIdAsync(matching.AppUserId, cancellationToken) ?? throw new NotFoundException(nameof(Domain.Entities.AppUser), matching.AppUserId);

        matching.Revoke();

        var (accessToken, accessExpires) = _tokenService.GenerateAccessToken(user);
        var newRefreshRaw = _tokenService.GenerateRandomToken(64);
        var newRefreshExpires = _tokenService.GetRefreshTokenExpiration();

        await _unitOfWork.AppUserRefreshTokens.AddAsync(new Domain.Entities.AppUserRefreshToken(user.Id, _passwordHasher.HashPassword(newRefreshRaw), newRefreshExpires, TokenType.RefreshToken, request.IpAddress), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new RefreshTokenResponseDto(accessToken, accessExpires, newRefreshRaw, newRefreshExpires);
        return ApiResponse.Success(dto, StatusCodes.Status200OK, "Token yenileme başarılı.");
    }
}
