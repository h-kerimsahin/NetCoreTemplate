using $safeprojectname$.Domain.Entities;

namespace $safeprojectname$.Domain.Interfaces.Repositories;

public interface IAppRolePermissionRepository : IRepository<AppRolePermission>, IDisposable
{
    Task<List<AppRolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<List<AppRolePermission>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
}
