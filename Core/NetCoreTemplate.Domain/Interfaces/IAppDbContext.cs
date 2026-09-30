using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Domain.Interfaces;

public interface IAppDbContext
{
    DbSet<AppUser> AppUsers { get; }
    DbSet<AppUserProfile> AppUserProfiles { get; }
    DbSet<AppUserRefreshToken> AppUserRefreshTokens { get; }
    DbSet<AppUserActivited> AppUserActivities { get; }
    DbSet<Setting> Settings { get; }
    DbSet<SystemLog> SystemLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
