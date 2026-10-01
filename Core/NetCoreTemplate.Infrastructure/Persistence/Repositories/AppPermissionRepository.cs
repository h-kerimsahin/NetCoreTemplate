using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces.Repositories;

namespace NetCoreTemplate.Infrastructure.Persistence.Repositories;

public class AppPermissionRepository : Repository<AppPermission>, IAppPermissionRepository
{
    public AppPermissionRepository(AppDbContext context) : base(context) { }

    public async Task<AppPermission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await GetWhere(p => p.Code == code).FirstOrDefaultAsync(cancellationToken);

    public async Task<List<AppPermission>> GetByGroupAsync(PermissionGroup groupName, CancellationToken cancellationToken = default)
        => await GetWhere(p => p.GroupName == groupName).OrderBy(p => p.Code).ToListAsync(cancellationToken);

    public async Task<List<AppPermission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        => await _context.AppRolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .ToListAsync(cancellationToken);

    public void Dispose() => _context?.Dispose();
}
