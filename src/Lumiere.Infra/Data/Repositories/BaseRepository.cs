using Lumiere.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Lumiere.Infra.Data.Repositories;

public abstract class BaseRepository<TEntity>(AppDbContext context) : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public IQueryable<TEntity> Get(params Expression<Func<TEntity, bool>>[] conditions)
    {

        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        foreach (var condition in conditions)
        {
            query = query.Where(condition);
        }

        return query;

    }

    public async Task<bool> Exists(CancellationToken cancellationToken, params Expression<Func<TEntity, bool>>[] conditions)
    {

        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        foreach (var condition in conditions)
        {
            query = query.Where(condition);
        }

        return await query.AnyAsync(cancellationToken);

    }

    public async Task Add(TEntity entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task Update(TEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

}
