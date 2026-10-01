using Microsoft.EntityFrameworkCore;
using $safeprojectname$.Domain.Entities;
using $safeprojectname$.Domain.Interfaces.Repositories;

namespace $safeprojectname$.Infrastructure.Persistence.Repositories;

public class AuditEntryRepository : Repository<AuditEntry>, IAuditEntryRepository
{
    public AuditEntryRepository(AppDbContext context) : base(context) { }

    public async Task<List<AuditEntry>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken cancellationToken = default)
        => await GetWhere(a => a.EntityName == entityName && a.EntityId == entityId).OrderByDescending(a => a.ChangedAt).ToListAsync(cancellationToken);

    public async Task<List<AuditEntry>> GetByUserIdAsync(Guid changedByUserId, CancellationToken cancellationToken = default)
        => await GetWhere(a => a.ChangedByUserId == changedByUserId).OrderByDescending(a => a.ChangedAt).ToListAsync(cancellationToken);

    public void Dispose() => _context?.Dispose();
}
