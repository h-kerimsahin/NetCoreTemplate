using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Interfaces.Repositories;

namespace $safeprojectname$.Infrastructure.Persistence.Repositories;

public class AppRoleRepository : Repository<AppRole>, IAppRoleRepository
{
    public AppRoleRepository(AppDbContext context) : base(context) { }

    public async Task<AppRole?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await GetWhere(r => r.Name == name).FirstOrDefaultAsync(cancellationToken);

    public async Task<List<AppRole>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.AppUserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .ToListAsync(cancellationToken);

    public void Dispose() => _context?.Dispose();
}
