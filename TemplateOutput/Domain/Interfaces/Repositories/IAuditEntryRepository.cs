using $safeprojectname$.Domain.Entities;

namespace $safeprojectname$.Domain.Interfaces.Repositories;

public interface IAuditEntryRepository : IRepository<AuditEntry>, IDisposable
{
    Task<List<AuditEntry>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken cancellationToken = default);
    Task<List<AuditEntry>> GetByUserIdAsync(Guid changedByUserId, CancellationToken cancellationToken = default);
}
