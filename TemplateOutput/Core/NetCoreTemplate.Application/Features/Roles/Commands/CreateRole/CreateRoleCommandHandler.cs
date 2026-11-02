using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NetCoreTemplate.Application.DTOs.Common;
using NetCoreTemplate.Application.Exceptions;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Security;

namespace NetCoreTemplate.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ApiResponse<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserActivityLogger _activityLogger;
    private readonly IMapper _mapper;

    public CreateRoleCommandHandler(IUnitOfWork unitOfWork, IUserActivityLogger activityLogger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var existingRole = await _unitOfWork.AppRoles.GetByNameAsync(request.Name, cancellationToken);
        if (existingRole != null)
        {
            throw new BusinessException($"'{request.Name}' rolü zaten mevcut.");
        }

        var role = AppRole.Create(request.Name, request.Description);
        await _unitOfWork.AppRoles.AddAsync(role, cancellationToken);

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

        await _activityLogger.LogAsync(null, UserActivityType.RoleAssigned, $"Rol oluşturuldu: {request.Name}", entityName: nameof(AppRole), entityId: role.Id, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roleWithPermissions = await _unitOfWork.AppRoles.GetByIdAsync(role.Id, cancellationToken);
        var dto = _mapper.Map<RoleDto>(roleWithPermissions);
        return ApiResponse.Success(dto, StatusCodes.Status201Created, "Rol başarıyla oluşturuldu.");
    }
}