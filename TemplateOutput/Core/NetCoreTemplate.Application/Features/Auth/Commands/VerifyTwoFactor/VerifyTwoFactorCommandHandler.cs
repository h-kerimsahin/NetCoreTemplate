using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;

namespace NetCoreTemplate.Application.Features.Auth.Commands.VerifyTwoFactor;

public class VerifyTwoFactorCommandHandler : IRequestHandler<VerifyTwoFactorCommand, ApiResponse<VerifyTwoFactorResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserActivityLogger _activityLogger;

    public VerifyTwoFactorCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService, IPasswordHasher passwordHasher, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<VerifyTwoFactorResponseDto>> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByEmailOrUserNameAsync(request.EmailOrUserName, cancellationToken) ?? throw new UnauthorizedException("Invalid credentials");
        var twoFactorToken = await _unitOfWork.AppUserRefreshTokens.GetWhere(t => t.AppUserId == user.Id && t.TokenType == TokenType.TwoFactor && !t.IsRevoked, false).FirstOrDefaultAsync(cancellationToken);

        if (twoFactorToken == null || twoFactorToken.Token != request.Code || twoFactorToken.ExpiresAt < DateTime.UtcNow)
            throw new BusinessException("Invalid or expired 2FA code");

        twoFactorToken.Revoke();

        var (accessToken, accessExpires) = _tokenService.GenerateAccessToken(user);
        var refreshRaw = _tokenService.GenerateRandomToken(64);
        var refreshExpires = _tokenService.GetRefreshTokenExpiration();

        await _unitOfWork.AppUserRefreshTokens.AddAsync(new Domain.Entities.AppUserRefreshToken(user.Id, _passwordHasher.HashPassword(refreshRaw), refreshExpires, TokenType.RefreshToken, request.IpAddress), cancellationToken);
        await _activityLogger.LogAsync(user.Id, UserActivityType.TwoFactorVerified, "2FA verified successfully", ipAddress: request.IpAddress, userAgent: request.UserAgent, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new VerifyTwoFactorResponseDto(accessToken, accessExpires, refreshRaw, refreshExpires);
        return ApiResponse.Success(dto, StatusCodes.Status200OK, "İki faktörlü doğrulama başarılı.");
    }
}
