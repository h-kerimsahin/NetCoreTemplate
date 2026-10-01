using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Application.DTOs.Common;
using $safeprojectname$.Application.Exceptions;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Domain.Interfaces.Security;

namespace $safeprojectname$.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, ApiResponse<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _activityLogger;
    private readonly IMapper _mapper;

    public UpdateRoleCommandHandler(IUnitOfWork unitOfWork, IUserActivityLogger activityLogger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.AppRoles.GetByIdAsync(request.Id, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(nameof(AppRole), request.Id);
        }

        if (role.Name != request.Name)
        {
            var existingRole = await _unitOfWork.AppRoles.GetByNameAsync(request.Name, cancellationToken);
            if (existingRole != null && existingRole.Id != request.Id)
            {
                throw new BusinessException($"'{request.Name}' rolü zaten mevcut.");
            }
        }

        role.UpdateDetails(request.Name, request.Description);
        _unitOfWork.AppRoles.Update(role);

        var existingPermissions = await _unitOfWork.AppRolePermissions.GetByRoleIdAsync(role.Id, cancellationToken);
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
                await _unitOfWork.AppRolePermissions.AddAsync(new AppRolePermission(role.Id, permissionId), cancellationToken);
            }
        }

        await _activityLogger.LogAsync(null, UserActivityType.PermissionGranted, $"Rol güncellendi: {request.Name}", entityName: nameof(AppRole), entityId: role.Id, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedRole = await _unitOfWork.AppRoles.GetByIdAsync(role.Id, cancellationToken);
        var dto = _mapper.Map<RoleDto>(updatedRole);
        return ApiResponse.Success(dto, StatusCodes.Status200OK, "Rol başarıyla güncellendi.");
    }
}