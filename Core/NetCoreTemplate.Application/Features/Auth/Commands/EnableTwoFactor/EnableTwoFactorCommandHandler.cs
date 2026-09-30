using MediatR;
using Microsoft.AspNetCore.Http;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;
using NetCoreTemplate.Domain.Interfaces.Services;

namespace NetCoreTemplate.Application.Features.Auth.Commands.EnableTwoFactor;

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _activityLogger;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;

    public EnableTwoFactorCommandHandler(IUnitOfWork unitOfWork, IUserActivityLogger activityLogger, IEmailService emailService, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<bool>> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByIdAsync(request.UserId, cancellationToken) ?? throw new NotFoundException(nameof(Domain.Entities.AppUser), request.UserId);
        var type = Enum.TryParse<TwoFactorType>(request.Type, true, out var t) ? t : TwoFactorType.Email;

        user.EnableTwoFactor(type);
        await _activityLogger.LogAsync(user.Id, UserActivityType.TwoFactorEnabled, $"2FA enabled via {type}", cancellationToken: cancellationToken);

        if (type == TwoFactorType.Email)
        {
            var code = _tokenService.GenerateRandomToken(6);
            var codeToken = new Domain.Entities.AppUserRefreshToken(user.Id, code, DateTime.UtcNow.AddMinutes(5), TokenType.TwoFactor);
            await _unitOfWork.AppUserRefreshTokens.AddAsync(codeToken, cancellationToken);
            try { await _emailService.SendTwoFactorCodeEmailAsync(user.Email, code, cancellationToken); } catch { }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success(true, StatusCodes.Status200OK, "İki faktörlü doğrulama etkinleştirildi.");
    }
}
