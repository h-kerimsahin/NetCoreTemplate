using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Interfaces.Repositories;

namespace NetCoreTemplate.Infrastructure.Persistence.Repositories;

public class AppRolePermissionRepository : Repository<AppRolePermission>, IAppRolePermissionRepository
{
    public AppRolePermissionRepository(AppDbContext context) : base(context) { }

    public async Task<List<AppRolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default) => await GetWhere(rp => rp.RoleId == roleId).ToListAsync(cancellationToken);

    public async Task<List<AppRolePermission>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default) => await GetWhere(rp => rp.PermissionId == permissionId).ToListAsync(cancellationToken);

    public async Task<bool> HasPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default) => await AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

    public void Dispose() => _context?.Dispose();
}
