using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;

namespace NetCoreTemplate.Domain.Interfaces.Repositories;

public interface IBackgroundJobLogRepository : IRepository<BackgroundJobLog>, IDisposable
{
    Task<List<BackgroundJobLog>> GetByStatusAsync(JobStatus status, CancellationToken cancellationToken = default);
    Task<List<BackgroundJobLog>> GetByJobTypeAsync(string jobType, CancellationToken cancellationToken = default);
    Task<int> GetFailedCountByJobTypeAsync(string jobType, int lastHours = 24, CancellationToken cancellationToken = default);
}
