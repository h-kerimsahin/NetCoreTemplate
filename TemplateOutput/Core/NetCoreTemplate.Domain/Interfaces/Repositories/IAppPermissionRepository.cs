using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;

namespace NetCoreTemplate.Domain.Interfaces.Repositories;

public interface IAppPermissionRepository : IRepository<AppPermission>, IDisposable
{
    Task<AppPermission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<AppPermission>> GetByGroupAsync(PermissionGroup groupName, CancellationToken cancellationToken = default);
    Task<List<AppPermission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
}
