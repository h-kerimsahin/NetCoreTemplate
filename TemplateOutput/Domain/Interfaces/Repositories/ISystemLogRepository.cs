using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Enums;

namespace $safeprojectname$.Domain.Interfaces.Repositories;

public interface ISystemLogRepository : IRepository<SystemLog>
{
    Task<List<SystemLog>> GetByLogLevelAsync(SystemLogLevel logLevel, int take = 100, CancellationToken cancellationToken = default);
    Task<List<SystemLog>> GetByUserIdAsync(Guid? userId, int take = 100, CancellationToken cancellationToken = default);
}
