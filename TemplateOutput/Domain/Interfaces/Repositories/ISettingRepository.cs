using $safeprojectname$.Domain.Entities;

namespace $safeprojectname$.Domain.Interfaces.Repositories;

public interface ISettingRepository : IRepository<Setting>
{
    Task<Setting?> GetByKeyAsync(string key, Guid? appUserId = null, CancellationToken cancellationToken = default);
    Task<string?> GetValueByKeyAsync(string key, Guid? appUserId = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string?>> GetAllGlobalSettingsAsync(CancellationToken cancellationToken = default);
}
