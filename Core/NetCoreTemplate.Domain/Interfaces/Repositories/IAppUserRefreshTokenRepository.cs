using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Domain.Interfaces.Repositories;

public interface IAppUserRefreshTokenRepository : IRepository<AppUserRefreshToken>
{
    Task<AppUserRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<AppUserRefreshToken>> GetActiveTokensByUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default);
    Task RevokeAllTokensForUserAsync(Guid appUserId, CancellationToken cancellationToken = default);
}
