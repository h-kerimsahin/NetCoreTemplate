using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Interfaces.Repositories;

namespace NetCoreTemplate.Infrastructure.Persistence.Repositories;

public class SettingRepository : Repository<Setting>, ISettingRepository
{
    public SettingRepository(AppDbContext context) : base(context) { }

    public async Task<Setting?> GetByKeyAsync(string key, Guid? appUserId = null, CancellationToken cancellationToken = default) => appUserId == null ? await GetWhere(s => s.Key == key && s.AppUserId == null).FirstOrDefaultAsync(cancellationToken) : await GetWhere(s => s.Key == key && s.AppUserId == appUserId).FirstOrDefaultAsync(cancellationToken);

    public async Task<string?> GetValueByKeyAsync(string key, Guid? appUserId = null, CancellationToken cancellationToken = default) => (await GetByKeyAsync(key, appUserId, cancellationToken))?.Value;

    public async Task<Dictionary<string, string?>> GetAllGlobalSettingsAsync(CancellationToken cancellationToken = default) => await GetWhere(s => s.AppUserId == null).ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);
}
