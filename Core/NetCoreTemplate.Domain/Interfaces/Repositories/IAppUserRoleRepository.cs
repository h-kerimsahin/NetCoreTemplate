using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Domain.Interfaces.Repositories;

public interface IAppUserRoleRepository : IRepository<AppUserRole>, IDisposable
{
    Task<List<AppUserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<AppUserRole>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> IsUserInRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}
