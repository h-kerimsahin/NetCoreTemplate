using $safeprojectname$.Domain.Entities;

namespace $safeprojectname$.Domain.Interfaces.Repositories;

public interface IAppNotificationRepository : IRepository<AppNotification>, IDisposable
{
    Task<List<AppNotification>> GetByUserIdAsync(Guid userId, bool includeGlobal = true, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
