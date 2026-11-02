using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces.Repositories;

namespace NetCoreTemplate.Infrastructure.Persistence.Repositories;

public class SystemLogRepository : Repository<SystemLog>, ISystemLogRepository
{
    public SystemLogRepository(AppDbContext context) : base(context) { }

    public async Task<List<SystemLog>> GetByLogLevelAsync(SystemLogLevel logLevel, int take = 100, CancellationToken cancellationToken = default) => await GetWhere(sl => sl.LogLevel == logLevel).OrderByDescending(sl => sl.CreatedDate).Take(take).ToListAsync(cancellationToken);

    public async Task<List<SystemLog>> GetByUserIdAsync(Guid? userId, int take = 100, CancellationToken cancellationToken = default) => await GetWhere(sl => sl.UserId == userId).OrderByDescending(sl => sl.CreatedDate).Take(take).ToListAsync(cancellationToken);
}
