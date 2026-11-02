using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NetCoreTemplate.Domain.Entities.Seedworks;
using NetCoreTemplate.Domain.Interfaces.Repositories;

namespace NetCoreTemplate.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public virtual IQueryable<T> GetAll(bool includeDeleted = false) => includeDeleted ? _set.IgnoreQueryFilters().AsNoTracking() : _set.AsNoTracking();

    public virtual IQueryable<T> GetWhere(Expression<Func<T, bool>> predicate, bool includeDeleted = false) => GetAll(includeDeleted).Where(predicate);

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => await _set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => await GetAll(false).FirstOrDefaultAsync(predicate, cancellationToken);

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => await _set.AsNoTracking().AnyAsync(predicate, cancellationToken);

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entry = await _set.AddAsync(entity, cancellationToken);
        return entry.Entity;
    }

    public virtual T Update(T entity)
    {
        var entry = _set.Update(entity);
        return entry.Entity;
    }

    public virtual void Delete(T entity) => _set.Remove(entity);

    public virtual void PermanentDelete(T entity)
    {
        var entry = _set.Attach(entity);
        entry.State = EntityState.Deleted;
        entry.Property(nameof(BaseEntity.Status)).IsModified = false;
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default) => predicate == null ? await _set.AsNoTracking().CountAsync(cancellationToken) : await _set.AsNoTracking().CountAsync(predicate, cancellationToken);
}
