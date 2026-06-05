using System.Linq.Expressions;
using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Lumiere.Infra.Repositories;

public class BaseRepository<TEntity>(AppDbContext context) : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task<IEnumerable<TEntity>> GetAllAsync(
        params Expression<Func<TEntity, bool>>[] conditions)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        foreach (var condition in conditions)
        {
            query = query.Where(condition);
        }

        return await query.ToListAsync();
    }

    public async Task<TEntity?> GetAsync(
        params Expression<Func<TEntity, bool>>[] conditions)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        foreach (var condition in conditions)
        {
            query = query.Where(condition);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
