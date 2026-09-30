using MediatR;
using Microsoft.AspNetCore.Http;
using NetCoreTemplate.Application.DTOs.Auth;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;

namespace NetCoreTemplate.Application.Features.Auth.Commands.Logout;

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
