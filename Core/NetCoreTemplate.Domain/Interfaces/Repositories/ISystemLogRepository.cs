using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;

namespace NetCoreTemplate.Domain.Interfaces.Repositories;

public interface ISystemLogRepository : IRepository<SystemLog>
{
    Task<List<SystemLog>> GetByLogLevelAsync(SystemLogLevel logLevel, int take = 100, CancellationToken cancellationToken = default);
    Task<List<SystemLog>> GetByUserIdAsync(Guid? userId, int take = 100, CancellationToken cancellationToken = default);
}
