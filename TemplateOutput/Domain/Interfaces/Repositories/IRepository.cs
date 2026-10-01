using System.Linq.Expressions;
using $safeprojectname$.Domain.Entities.Seedworks;

namespace $safeprojectname$.Domain.Interfaces.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    IQueryable<T> GetAll(bool includeDeleted = false);
    IQueryable<T> GetWhere(Expression<Func<T, bool>> predicate, bool includeDeleted = false);
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    T Update(T entity);
    void Delete(T entity);
    void PermanentDelete(T entity);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
}
