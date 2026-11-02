using NetCoreTemplate.Domain.Interfaces.Repositories;

namespace NetCoreTemplate.Domain.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IAppUserRepository AppUsers { get; }
    IAppUserProfileRepository AppUserProfiles { get; }
    IAppUserRefreshTokenRepository AppUserRefreshTokens { get; }
    IAppUserActivitedRepository AppUserActivities { get; }
    ISettingRepository Settings { get; }
    ISystemLogRepository SystemLogs { get; }
    IAppRoleRepository AppRoles { get; }
    IAppUserRoleRepository AppUserRoles { get; }
    IAppPermissionRepository AppPermissions { get; }
    IAppRolePermissionRepository AppRolePermissions { get; }
    IAuditEntryRepository AuditEntries { get; }
    IAppNotificationRepository AppNotifications { get; }
    IBackgroundJobLogRepository BackgroundJobLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
