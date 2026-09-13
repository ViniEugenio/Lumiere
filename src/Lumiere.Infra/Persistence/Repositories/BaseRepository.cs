using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Lumiere.Infra.Persistence.Repositories;

public abstract class BaseRepository<TEntity>(AppDbContext context) : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken, params Expression<Func<TEntity, bool>>[] conditions)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        foreach (var condition in conditions)
        {
            query = query.Where(condition);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetAsync(CancellationToken cancellationToken, params Expression<Func<TEntity, bool>>[] conditions)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        foreach (var condition in conditions)
        {
            query = query.Where(condition);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(CancellationToken cancellationToken, params Expression<Func<TEntity, bool>>[] conditions)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        foreach (var condition in conditions)
        {
            query = query.Where(condition);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

}
