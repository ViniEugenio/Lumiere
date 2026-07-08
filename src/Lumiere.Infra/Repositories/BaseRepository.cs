using Lumiere.Domain.Common;
using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Lumiere.Infra.Repositories;

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

    public async Task<BasePaginationResult<TResult>> GetAllPaginationAsync<TResult>(PaginationFilters<TEntity, TResult> filters, CancellationToken cancellationToken)
    {
        IQueryable<TEntity> query = _dbSet
            .AsNoTracking()
            .Where(filters.FilterExpression);

        int totalItems = await query
            .CountAsync(cancellationToken);

        int totalPages = (int)Math.Ceiling((decimal)totalItems / filters.PageAmount);

        int skip = (filters.Page - 1) * filters.PageAmount;

        List<TResult> items = await query
            .OrderBy(filters.OrderByExpression)
            .Select(filters.SelectorExpression)
            .Skip(skip)
            .Take(filters.PageAmount)
            .ToListAsync(cancellationToken);

        return new BasePaginationResult<TResult>(filters.Page, filters.PageAmount, totalPages, items);
    }
}
