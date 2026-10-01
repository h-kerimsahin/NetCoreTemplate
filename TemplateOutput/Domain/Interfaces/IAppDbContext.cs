using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Domain.Entities;

namespace $safeprojectname$.Domain.Interfaces;

public interface IAppDbContext
{
    DbSet<AppUser> AppUsers { get; }
    DbSet<AppUserProfile> AppUserProfiles { get; }
    DbSet<AppUserRefreshToken> AppUserRefreshTokens { get; }
    DbSet<AppUserActivited> AppUserActivities { get; }
    DbSet<Setting> Settings { get; }
    DbSet<SystemLog> SystemLogs { get; }
    DbSet<AppRole> AppRoles { get; }
    DbSet<AppUserRole> AppUserRoles { get; }
    DbSet<AppPermission> AppPermissions { get; }
    DbSet<AppRolePermission> AppRolePermissions { get; }
    DbSet<AuditEntry> AuditEntries { get; }
    DbSet<AppNotification> AppNotifications { get; }
    DbSet<BackgroundJobLog> BackgroundJobLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
