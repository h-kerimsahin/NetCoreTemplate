using MediatR;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;
using NetCoreTemplate.Domain.Interfaces.Services;

namespace NetCoreTemplate.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IUserActivityLogger _activityLogger;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, ITokenService tokenService, IUserActivityLogger activityLogger, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _activityLogger = activityLogger;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByEmailAsync(request.Email, cancellationToken);
        if (user != null)
        {
            var resetTokenRaw = _tokenService.GenerateRandomToken(48);
            var resetToken = new Domain.Entities.AppUserRefreshToken(user.Id, resetTokenRaw, DateTime.UtcNow.AddHours(1), TokenType.ResetPassword);

            await _unitOfWork.AppUserRefreshTokens.AddAsync(resetToken, cancellationToken);
            await _activityLogger.LogAsync(user.Id, UserActivityType.ForgotPasswordRequested, "Password reset requested", cancellationToken: cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var resetLink = $"https://localhost:7000/reset-password?token={resetTokenRaw}&email={Uri.EscapeDataString(request.Email)}";
            try { await _emailService.SendResetPasswordEmailAsync(request.Email, resetLink, cancellationToken); } catch { }
        }
        return true;
    }
}
