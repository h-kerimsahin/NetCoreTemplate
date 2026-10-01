using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Interfaces;

namespace NetCoreTemplate.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _unitOfWork.AppNotifications.GetWhere(n => n.Id == request.NotificationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (notification == null)
        {
            throw new NotFoundException(nameof(AppNotification), request.NotificationId);
        }

        if (notification.UserId.HasValue && notification.UserId.Value != request.UserId)
        {
            throw new UnauthorizedAccessException("Bu bildirimi okuma izniniz yok.");
        }

        notification.MarkAsRead();
        _unitOfWork.AppNotifications.Update(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(true, StatusCodes.Status200OK, "Bildirim okundu olarak işaretlendi.");
    }
}