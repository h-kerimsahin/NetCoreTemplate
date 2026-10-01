using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.UserManagement.Commands.AssignRoleToUser;

public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _activityLogger;

    public AssignRoleToUserCommandHandler(IUnitOfWork unitOfWork, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<bool>> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.AppUsers.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(AppUser), request.UserId);
        }

        var existingUserRoles = await _unitOfWork.AppUserRoles.GetByUserIdAsync(request.UserId, cancellationToken);
        foreach (var existing in existingUserRoles)
        {
            _unitOfWork.AppUserRoles.Delete(existing);
        }

        if (request.RoleIds != null && request.RoleIds.Count != 0)
        {
            foreach (var roleId in request.RoleIds)
            {
                var role = await _unitOfWork.AppRoles.GetByIdAsync(roleId, cancellationToken);
                if (role == null)
                {
                    throw new NotFoundException(nameof(AppRole), roleId);
                }
                await _unitOfWork.AppUserRoles.AddAsync(new AppUserRole(request.UserId, roleId), cancellationToken);
            }
        }

        await _activityLogger.LogAsync(user.Id, UserActivityType.RoleAssigned, $"Kullanıcıya roller atandı: {string.Join(", ", request.RoleIds ?? new List<Guid>())}", entityName: nameof(AppUser), entityId: user.Id, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(true, StatusCodes.Status200OK, "Roller başarıyla kullanıcıya atandı.");
    }
}