using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Auth;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUserActivityLogger _activityLogger;
    private readonly IMapper _mapper;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenService tokenService, IUserActivityLogger activityLogger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _activityLogger = activityLogger;
        _mapper = mapper;
    }

    public async Task<ApiResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByEmailOrUserNameAsync(request.EmailOrUserName, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedException("Geçersiz giriş bilgileri");
        }

        if (user.IsLockedOut)
        {
            await _activityLogger.LogAsync(user.Id, UserActivityType.AccountLocked, "Account locked due to failed login attempts", ipAddress: request.IpAddress, userAgent: request.UserAgent, cancellationToken: cancellationToken);
            throw new UnauthorizedException($"Hesap şu ana kadar kilitlendi: {user.LockoutEndDate:O}");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _activityLogger.LogAsync(user.Id, UserActivityType.LoginFailed, $"Failed login attempt from {request.IpAddress}", ipAddress: request.IpAddress, userAgent: request.UserAgent, cancellationToken: cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Geçersiz giriş bilgileri");
        }

        user.RecordSuccessfulLogin();
        user.ResetAccessFailedCount();

        var (accessToken, accessExpires) = _tokenService.GenerateAccessToken(user);
        var refreshTokenRaw = _tokenService.GenerateRandomToken(64);
        var refreshExpires = _tokenService.GetRefreshTokenExpiration();
        var refreshToken = new Domain.Entities.AppUserRefreshToken(user.Id, _passwordHasher.HashPassword(refreshTokenRaw), refreshExpires, TokenType.RefreshToken, request.IpAddress);

        await _unitOfWork.AppUserRefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _activityLogger.LogAsync(user.Id, UserActivityType.Login, $"Successful login from {request.IpAddress}", ipAddress: request.IpAddress, userAgent: request.UserAgent, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new LoginResponseDto(accessToken, accessExpires, refreshTokenRaw, refreshExpires);
        return ApiResponse.Success(dto, StatusCodes.Status200OK, "Giriş başarılı.");
    }
}
