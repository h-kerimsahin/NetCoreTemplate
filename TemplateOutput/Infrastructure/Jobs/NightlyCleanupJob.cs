using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Domain.Interfaces;
using $safeprojectname$.Infrastructure.Persistence;

namespace $safeprojectname$.Infrastructure.Jobs;

public class NightlyCleanupJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _dbContext;

    public NightlyCleanupJob(IUnitOfWork unitOfWork, AppDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task Execute(CancellationToken ct)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        await _dbContext.SystemLogs
            .IgnoreQueryFilters()
            .Where(sl => sl.CreatedDate < thirtyDaysAgo)
            .ExecuteDeleteAsync(ct);

        var now = DateTime.UtcNow;
        await _dbContext.AppUserRefreshTokens
            .IgnoreQueryFilters()
            .Where(rt => rt.ExpiresAt < now && !rt.IsRevoked)
            .ExecuteDeleteAsync(ct);

        var lockoutExpiry = now.AddHours(-24);
        var usersToReset = await _dbContext.AppUsers
            .Where(u => u.LockoutEndDate < lockoutExpiry && u.AccessFailedCount > 0)
            .ToListAsync(ct);

        foreach (var user in usersToReset)
        {
            user.ResetAccessFailedCount();
            _dbContext.AppUsers.Update(user);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var yearAgo = now.AddDays(-365);
        await _dbContext.AuditEntries
            .IgnoreQueryFilters()
            .Where(ae => ae.CreatedDate < yearAgo)
            .ExecuteDeleteAsync(ct);
    }
}
