using System.Linq.Expressions;

namespace Lumiere.Application.Interfaces.Repositories;

public interface IBaseRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Get(params Expression<Func<TEntity, bool>>[] conditions);
    Task<bool> Exists(CancellationToken cancellationToken, params Expression<Func<TEntity, bool>>[] conditions);
    Task Add(TEntity entity, CancellationToken cancellationToken);
    Task Update(TEntity entity, CancellationToken cancellationToken);
}
