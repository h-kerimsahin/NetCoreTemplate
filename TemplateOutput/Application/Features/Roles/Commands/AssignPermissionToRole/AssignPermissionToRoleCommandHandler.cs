using MediatR;
using Microsoft.AspNetCore.Http;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.Roles.Commands.AssignPermissionToRole;

public class AssignPermissionToRoleCommandHandler : IRequestHandler<AssignPermissionToRoleCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _activityLogger;

    public AssignPermissionToRoleCommandHandler(IUnitOfWork unitOfWork, IUserActivityLogger activityLogger)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<ApiResponse<bool>> Handle(AssignPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.AppRoles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(nameof(AppRole), request.RoleId);
        }

        var existingPermissions = await _unitOfWork.AppRolePermissions.GetByRoleIdAsync(request.RoleId, cancellationToken);
        foreach (var existing in existingPermissions)
        {
            _unitOfWork.AppRolePermissions.Delete(existing);
        }

        if (request.PermissionIds != null && request.PermissionIds.Count != 0)
        {
            foreach (var permissionId in request.PermissionIds)
            {
                var permission = await _unitOfWork.AppPermissions.GetByIdAsync(permissionId, cancellationToken);
                if (permission == null)
                {
                    throw new NotFoundException(nameof(AppPermission), permissionId);
                }
                await _unitOfWork.AppRolePermissions.AddAsync(new AppRolePermission(request.RoleId, permissionId), cancellationToken);
            }
        }

        await _activityLogger.LogAsync(null, UserActivityType.PermissionGranted, $"Rol için izinler atandı: {role.Name}", entityName: nameof(AppRole), entityId: role.Id, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success(true, StatusCodes.Status200OK, "İzinler başarıyla role atandı.");
    }
}