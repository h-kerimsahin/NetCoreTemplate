using $safeprojectname$.Domain.Entities;

namespace $safeprojectname$.Domain.Interfaces.Repositories;

public interface IAppRoleRepository : IRepository<AppRole>, IDisposable
{
    Task<AppRole?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<AppRole>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
