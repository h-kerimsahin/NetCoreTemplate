using MediatR;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.UserManagement.Commands.UnlockUser;

public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _activityLogger;

    public UnlockUserCommandHandler(IUnitOfWork unitOfWork, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<bool>> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(AppUser), request.UserId);
        }

        user.ResetAccessFailedCount();

        var lockoutField = typeof(AppUser).GetField("<LockoutEndDate>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        if (lockoutField != null)
        {
            lockoutField.SetValue(user, null);
        }

        _unitOfWork.AppUsers.Update(user);

        await _activityLogger.LogAsync(user.Id, UserActivityType.RoleAssigned, "Kullanıcı hesabının kilidi açıldı", entityName: nameof(AppUser), entityId: user.Id, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(true, StatusCodes.Status200OK, "Kullanıcı hesabının kilidi başarıyla açıldı.");
    }
}