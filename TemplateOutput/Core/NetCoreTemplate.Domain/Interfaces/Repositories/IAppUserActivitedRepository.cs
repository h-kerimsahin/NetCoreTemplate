using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;

namespace NetCoreTemplate.Domain.Interfaces.Repositories;

public interface IAppUserActivitedRepository : IRepository<AppUserActivited>
{
    Task<List<AppUserActivited>> GetByUserIdAsync(Guid appUserId, int take = 50, CancellationToken cancellationToken = default);
    Task<List<AppUserActivited>> GetByActivityTypeAsync(UserActivityType activityType, int take = 50, CancellationToken cancellationToken = default);
}
