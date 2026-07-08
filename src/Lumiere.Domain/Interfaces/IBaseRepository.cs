using Lumiere.Domain.Common;
using System.Linq.Expressions;

namespace Lumiere.Domain.Interfaces;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken, params Expression<Func<TEntity, bool>>[] conditions);
    Task<TEntity?> GetAsync(CancellationToken cancellationToken, params Expression<Func<TEntity, bool>>[] conditions);
    Task<bool> ExistsAsync(CancellationToken cancellationToken, params Expression<Func<TEntity, bool>>[] conditions);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<BasePaginationResult<TResult>> GetAllPaginationAsync<TResult>(PaginationFilters<TEntity, TResult> filters, CancellationToken cancellationToken);
}
