using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Interfaces.Repositories;

namespace $safeprojectname$.Infrastructure.Persistence.Repositories;

public class AppUserRoleRepository : Repository<AppUserRole>, IAppUserRoleRepository
{
    public AppUserRoleRepository(AppDbContext context) : base(context) { }

    public async Task<List<AppUserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => await GetWhere(ur => ur.UserId == userId).ToListAsync(cancellationToken);

    public async Task<List<AppUserRole>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default) => await GetWhere(ur => ur.RoleId == roleId).ToListAsync(cancellationToken);

    public async Task<bool> IsUserInRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default) => await AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

    public void Dispose() => _context?.Dispose();
}
