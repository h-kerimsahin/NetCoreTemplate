using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Interfaces.Repositories;

namespace $safeprojectname$.Infrastructure.Persistence.Repositories;

public class AppNotificationRepository : Repository<AppNotification>, IAppNotificationRepository
{
    public AppNotificationRepository(AppDbContext context) : base(context) { }

    public async Task<List<AppNotification>> GetByUserIdAsync(Guid userId, bool includeGlobal = true, CancellationToken cancellationToken = default)
    {
        var query = GetWhere(n => n.UserId == userId);
        if (includeGlobal)
        {
            query = query.Concat(GetWhere(n => n.UserId == null));
        }
        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetWhere(n => n.UserId == userId && !n.IsRead).CountAsync(cancellationToken)
             + await GetWhere(n => n.UserId == null && !n.IsRead).CountAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await GetWhere(n => (n.UserId == userId || n.UserId == null) && !n.IsRead).ToListAsync(cancellationToken);
        foreach (var n in notifications) n.MarkAsRead();
        _context.UpdateRange(notifications);
    }

    public void Dispose() => _context?.Dispose();
}
