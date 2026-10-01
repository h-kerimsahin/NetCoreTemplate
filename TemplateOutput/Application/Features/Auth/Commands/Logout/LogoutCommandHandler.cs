using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Auth;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<LogoutResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _activityLogger;

    public LogoutCommandHandler(IUnitOfWork unitOfWork, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<LogoutResponse>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.AppUserRefreshTokens.RevokeAllTokensForUserAsync(request.UserId, cancellationToken);
        await _activityLogger.LogAsync(request.UserId, UserActivityType.Logout, $"User logged out from {request.IpAddress}", ipAddress: request.IpAddress, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new LogoutResponse(true);
        return ApiResponse.Success(dto, StatusCodes.Status200OK, "Çıkış başarılı.");
    }
}
