using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Interfaces.Repositories;

namespace $safeprojectname$.Infrastructure.Persistence.Repositories;

public class AppUserRefreshTokenRepository : Repository<AppUserRefreshToken>, IAppUserRefreshTokenRepository
{
    public AppUserRefreshTokenRepository(AppDbContext context) : base(context) { }

    public async Task<AppUserRefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) => await GetWhere(rt => rt.Token == token).FirstOrDefaultAsync(cancellationToken);

    public async Task<List<AppUserRefreshToken>> GetActiveTokensByUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default) => await GetWhere(rt => rt.AppUserId == appUserId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow).ToListAsync(cancellationToken);

    public async Task RevokeAllTokensForUserAsync(Guid appUserId, CancellationToken cancellationToken = default)
    {
        var tokens = await GetActiveTokensByUserIdAsync(appUserId, cancellationToken);
        foreach (var t in tokens) t.Revoke();
        if (tokens.Count > 0) _context.UpdateRange(tokens);
    }
}
