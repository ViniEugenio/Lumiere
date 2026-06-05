using System.Linq.Expressions;

namespace Lumiere.Domain.Interfaces;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync(
        params Expression<Func<TEntity, bool>>[] conditions);

    Task<TEntity?> GetAsync(
        params Expression<Func<TEntity, bool>>[] conditions);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
}
