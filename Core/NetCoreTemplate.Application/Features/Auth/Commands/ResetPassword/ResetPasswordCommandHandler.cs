using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;

namespace NetCoreTemplate.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserActivityLogger _activityLogger;

    public ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _activityLogger = activityLogger;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByEmailAsync(request.Email, cancellationToken) ?? throw new NotFoundException("User with email not found");
        var resetToken = await _unitOfWork.AppUserRefreshTokens.GetWhere(t => t.AppUserId == user.Id && t.TokenType == TokenType.ResetPassword && !t.IsRevoked, false).FirstOrDefaultAsync(cancellationToken);

        if (resetToken == null || resetToken.Token != request.Token || resetToken.ExpiresAt < DateTime.UtcNow)
            throw new BusinessException("Invalid or expired reset token");

        user.UpdatePassword(_passwordHasher.HashPassword(request.NewPassword));
        resetToken.Revoke();

        await _activityLogger.LogAsync(user.Id, UserActivityType.PasswordReset, "Password reset successfully", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
