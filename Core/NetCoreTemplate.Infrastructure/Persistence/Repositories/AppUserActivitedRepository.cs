using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces.Repositories;

namespace NetCoreTemplate.Infrastructure.Persistence.Repositories;

public class AppUserActivitedRepository : Repository<AppUserActivited>, IAppUserActivitedRepository
{
    public AppUserActivitedRepository(AppDbContext context) : base(context) { }

    public async Task<List<AppUserActivited>> GetByUserIdAsync(Guid appUserId, int take = 50, CancellationToken cancellationToken = default) => await GetWhere(a => a.AppUserId == appUserId).OrderByDescending(a => a.CreatedDate).Take(take).ToListAsync(cancellationToken);

    public async Task<List<AppUserActivited>> GetByActivityTypeAsync(UserActivityType activityType, int take = 50, CancellationToken cancellationToken = default) => await GetWhere(a => a.ActivityType == activityType).OrderByDescending(a => a.CreatedDate).Take(take).ToListAsync(cancellationToken);
}
