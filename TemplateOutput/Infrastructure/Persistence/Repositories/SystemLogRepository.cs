using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;
using $safeprojectname$.Domain.Interfaces.Repositories;

namespace $safeprojectname$.Infrastructure.Persistence.Repositories;

public class SystemLogRepository : Repository<SystemLog>, ISystemLogRepository
{
    public SystemLogRepository(AppDbContext context) : base(context) { }

    public async Task<List<SystemLog>> GetByLogLevelAsync(SystemLogLevel logLevel, int take = 100, CancellationToken cancellationToken = default) => await GetWhere(sl => sl.LogLevel == logLevel).OrderByDescending(sl => sl.CreatedDate).Take(take).ToListAsync(cancellationToken);

    public async Task<List<SystemLog>> GetByUserIdAsync(Guid? userId, int take = 100, CancellationToken cancellationToken = default) => await GetWhere(sl => sl.UserId == userId).OrderByDescending(sl => sl.CreatedDate).Take(take).ToListAsync(cancellationToken);
}
