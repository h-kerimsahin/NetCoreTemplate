using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Interfaces.Repositories;

namespace $safeprojectname$.Infrastructure.Persistence.Repositories;

public class AppUserProfileRepository : Repository<AppUserProfile>, IAppUserProfileRepository
{
    public AppUserProfileRepository(AppDbContext context) : base(context) { }

    public async Task<AppUserProfile?> GetByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default) => await GetWhere(p => p.AppUserId == appUserId).FirstOrDefaultAsync(cancellationToken);

    public async Task<AppUserProfile?> GetByIdWithAppUserAsync(Guid id, CancellationToken cancellationToken = default) => await _set.AsNoTracking().Include(p => p.AppUser).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}
