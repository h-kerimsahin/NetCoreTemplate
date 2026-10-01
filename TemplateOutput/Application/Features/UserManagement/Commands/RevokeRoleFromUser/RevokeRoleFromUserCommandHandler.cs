using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.UserManagement.Commands.RevokeRoleFromUser;

public class RevokeRoleFromUserCommandHandler : IRequestHandler<RevokeRoleFromUserCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _activityLogger;

    public RevokeRoleFromUserCommandHandler(IUnitOfWork unitOfWork, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<bool>> Handle(RevokeRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(AppUser), request.UserId);
        }

        var role = await _unitOfWork.AppRoles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(nameof(AppRole), request.RoleId);
        }

        var userRole = await _unitOfWork.AppUserRoles.GetWhere(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (userRole != null)
        {
            _unitOfWork.AppUserRoles.Delete(userRole);
        }

        await _activityLogger.LogAsync(user.Id, UserActivityType.RoleRevoked, $"Kullanıcıdan rol geri alındı: {role.Name}", entityName: nameof(AppUser), entityId: user.Id, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(true, StatusCodes.Status200OK, "Rol başarıyla kullanıcıdan geri alındı.");
    }
}