using $safeprojectname$.Domain.Entities;

namespace $safeprojectname$.Domain.Interfaces.Repositories;

public interface IAppUserProfileRepository : IRepository<AppUserProfile>
{
    Task<AppUserProfile?> GetByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default);
    Task<AppUserProfile?> GetByIdWithAppUserAsync(Guid id, CancellationToken cancellationToken = default);
}
