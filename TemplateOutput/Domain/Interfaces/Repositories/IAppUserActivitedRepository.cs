using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;

namespace $safeprojectname$.Domain.Interfaces.Repositories;

public interface IAppUserActivitedRepository : IRepository<AppUserActivited>
{
    Task<List<AppUserActivited>> GetByUserIdAsync(Guid appUserId, int take = 50, CancellationToken cancellationToken = default);
    Task<List<AppUserActivited>> GetByActivityTypeAsync(UserActivityType activityType, int take = 50, CancellationToken cancellationToken = default);
}
