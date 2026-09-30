using NetCoreTemplate.Domain.Interfaces;
using NetCoreTemplate.Domain.Interfaces.Repositories;
using NetCoreTemplate.Infrastructure.Persistence.Repositories;

namespace NetCoreTemplate.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IAppUserRepository? _appUsers;
    private IAppUserProfileRepository? _appUserProfiles;
    private IAppUserRefreshTokenRepository? _appUserRefreshTokens;
    private IAppUserActivitedRepository? _appUserActivities;
    private ISettingRepository? _settings;
    private ISystemLogRepository? _systemLogs;

    public UnitOfWork(AppDbContext context) => _context = context;

    public IAppUserRepository AppUsers => _appUsers ??= new AppUserRepository(_context);
    public IAppUserProfileRepository AppUserProfiles => _appUserProfiles ??= new AppUserProfileRepository(_context);
    public IAppUserRefreshTokenRepository AppUserRefreshTokens => _appUserRefreshTokens ??= new AppUserRefreshTokenRepository(_context);
    public IAppUserActivitedRepository AppUserActivities => _appUserActivities ??= new AppUserActivitedRepository(_context);
    public ISettingRepository Settings => _settings ??= new SettingRepository(_context);
    public ISystemLogRepository SystemLogs => _systemLogs ??= new SystemLogRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await _context.SaveChangesAsync(cancellationToken);

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
}
