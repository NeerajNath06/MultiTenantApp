using System.Linq.Expressions;
using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    /// <summary>Same as FindAsync but ignores global query filters (e.g. for login before tenant is set).</summary>
    Task<IEnumerable<T>> FindIgnoreFiltersAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    /// <summary>Same as FirstOrDefaultAsync but ignores global query filters (e.g. for login before tenant is set).</summary>
    Task<T?> FirstOrDefaultIgnoreFiltersAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    IQueryable<T> GetQueryable();
    Task<IEnumerable<T>> GetPagedAsync(IQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
