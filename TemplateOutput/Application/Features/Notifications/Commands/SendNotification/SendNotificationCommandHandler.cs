using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;
using $safeprojectname$.Domain.Interfaces.Services;

namespace $safeprojectname$.Application.Features.Notifications.Commands.SendNotification;

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IUserActivityLogger _activityLogger;

    public SendNotificationCommandHandler(IUnitOfWork unitOfWork, INotificationService notificationService, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<bool>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId.HasValue)
        {
            await _notificationService.SendToUserAsync(request.UserId.Value, request.Title, request.Message, request.Type, request.Data, cancellationToken);
            await _activityLogger.LogAsync(request.UserId.Value, UserActivityType.NotificationSent, $"Bildirim gönderildi: {request.Title}", cancellationToken: cancellationToken);
        }
        else
        {
            await _notificationService.SendToAllAsync(request.Title, request.Message, request.Type, request.Data, cancellationToken);
            await _activityLogger.LogAsync(null, UserActivityType.NotificationSent, $"Genel bildirim gönderildi: {request.Title}", cancellationToken: cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(true, StatusCodes.Status200OK, "Bildirim başarıyla gönderildi.");
    }
}