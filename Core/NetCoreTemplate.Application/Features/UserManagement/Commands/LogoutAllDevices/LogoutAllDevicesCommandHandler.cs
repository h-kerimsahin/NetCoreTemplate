using MediatR;
using Microsoft.AspNetCore.Http;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;

namespace NetCoreTemplate.Application.Features.UserManagement.Commands.LogoutAllDevices;

public class LogoutAllDevicesCommandHandler : IRequestHandler<LogoutAllDevicesCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _activityLogger;

    public LogoutAllDevicesCommandHandler(IUnitOfWork unitOfWork, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<bool>> Handle(LogoutAllDevicesCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(AppUser), request.UserId);
        }

        var userField = typeof(AppUser).GetField("<SecurityStamp>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (userField != null)
        {
            userField.SetValue(user, Guid.NewGuid().ToString());
        }

        _unitOfWork.AppUsers.Update(user);

        await _activityLogger.LogAsync(user.Id, UserActivityType.SecurityStampChanged, "Tüm cihazlardan çıkış yapıldı (SecurityStamp yenilendi)", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(true, StatusCodes.Status200OK, "Tüm cihazlardan çıkış başarıyla yapıldı.");
    }
}