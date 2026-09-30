using AutoMapper;
using MediatR;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;

namespace NetCoreTemplate.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
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

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByEmailOrUserNameAsync(request.EmailOrUserName, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        if (user.IsLockedOut)
        {
            await _activityLogger.LogAsync(user.Id, UserActivityType.AccountLocked, "Account locked due to failed login attempts", ipAddress: request.IpAddress, userAgent: request.UserAgent, cancellationToken: cancellationToken);
            throw new UnauthorizedException($"Account is locked out until {user.LockoutEndDate:O}");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _activityLogger.LogAsync(user.Id, UserActivityType.LoginFailed, $"Failed login attempt from {request.IpAddress}", ipAddress: request.IpAddress, userAgent: request.UserAgent, cancellationToken: cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid credentials");
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

        return new LoginResponseDto(accessToken, accessExpires, refreshTokenRaw, refreshExpires);
    }
}
