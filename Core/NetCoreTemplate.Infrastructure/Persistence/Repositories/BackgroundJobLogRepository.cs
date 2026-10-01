using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Domain.Entities;
using NetCoreTemplate.Domain.Enums;
using NetCoreTemplate.Domain.Interfaces.Repositories;

namespace NetCoreTemplate.Infrastructure.Persistence.Repositories;

public class BackgroundJobLogRepository : Repository<BackgroundJobLog>, IBackgroundJobLogRepository
{
    public BackgroundJobLogRepository(AppDbContext context) : base(context) { }

    public async Task<List<BackgroundJobLog>> GetByStatusAsync(JobStatus status, CancellationToken cancellationToken = default)
        => await GetWhere(j => j.JobStatus == status).OrderByDescending(j => j.CreatedDate).ToListAsync(cancellationToken);

    public async Task<List<BackgroundJobLog>> GetByJobTypeAsync(string jobType, CancellationToken cancellationToken = default)
        => await GetWhere(j => j.JobType == jobType).OrderByDescending(j => j.CreatedDate).ToListAsync(cancellationToken);

    public async Task<int> GetFailedCountByJobTypeAsync(string jobType, int lastHours = 24, CancellationToken cancellationToken = default)
    {
        var since = DateTime.UtcNow.AddHours(-lastHours);
        return await GetWhere(j => j.JobType == jobType && j.JobStatus == JobStatus.Failed && j.CreatedDate >= since).CountAsync(cancellationToken);
    }

    public void Dispose() => _context?.Dispose();
}
