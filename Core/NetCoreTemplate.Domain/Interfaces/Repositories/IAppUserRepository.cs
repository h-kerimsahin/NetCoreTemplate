using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Domain.Interfaces.Repositories;

public interface IAppUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AppUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<AppUser?> GetByEmailOrUserNameAsync(string emailOrUserName, CancellationToken cancellationToken = default);
    Task<AppUser?> GetByIdWithProfileAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, Guid? exceptUserId = null, CancellationToken cancellationToken = default);
    Task<bool> IsUserNameExistsAsync(string userName, Guid? exceptUserId = null, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetUserIdsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetAllUserIdsAsync(CancellationToken cancellationToken = default);
    Task<Guid?> GetUserIdByConnectionIdAsync(string connectionId, CancellationToken cancellationToken = default);
}
