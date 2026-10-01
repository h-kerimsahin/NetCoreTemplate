using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Interfaces.Repositories;

namespace $safeprojectname$.Infrastructure.Persistence.Repositories;

public class AppUserRepository : Repository<AppUser>, IAppUserRepository
{
    public AppUserRepository(AppDbContext context) : base(context) { }

    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => await GetWhere(u => u.Email == email).FirstOrDefaultAsync(cancellationToken);

    public async Task<AppUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default) => await GetWhere(u => u.UserName == userName).FirstOrDefaultAsync(cancellationToken);

    public async Task<AppUser?> GetByEmailOrUserNameAsync(string emailOrUserName, CancellationToken cancellationToken = default) => await GetWhere(u => u.Email == emailOrUserName || u.UserName == emailOrUserName).FirstOrDefaultAsync(cancellationToken);

    public async Task<AppUser?> GetByIdWithProfileAsync(Guid id, CancellationToken cancellationToken = default) => await _set.AsNoTracking().Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> IsEmailExistsAsync(string email, Guid? exceptUserId = null, CancellationToken cancellationToken = default) => exceptUserId == null ? await AnyAsync(u => u.Email == email, cancellationToken) : await AnyAsync(u => u.Email == email && u.Id != exceptUserId, cancellationToken);

    public async Task<bool> IsUserNameExistsAsync(string userName, Guid? exceptUserId = null, CancellationToken cancellationToken = default) => exceptUserId == null ? await AnyAsync(u => u.UserName == userName, cancellationToken) : await AnyAsync(u => u.UserName == userName && u.Id != exceptUserId, cancellationToken);

    public async Task<List<Guid>> GetUserIdsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        => await _context.AppUserRoles
            .AsNoTracking()
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<List<Guid>> GetAllUserIdsAsync(CancellationToken cancellationToken = default)
        => await GetAll().Select(u => u.Id).ToListAsync(cancellationToken);

    public async Task<Guid?> GetUserIdByConnectionIdAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        return await _context.AppUserActivities
            .AsNoTracking()
            .Where(a => a.Description != null && a.Description.Contains(connectionId))
            .OrderByDescending(a => a.CreatedDate)
            .Select(a => a.AppUserId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
