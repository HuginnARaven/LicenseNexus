using System.Linq.Expressions;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class BaseSqlRepository<T>: IBaseRepository<T> where T : class, IEntity
{
    protected readonly BaseSqlContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseSqlRepository(BaseSqlContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(int id, bool trackChanges = false, params Expression<Func<T, object>>[] includes)
    {
        bool hasIncludes = includes != null && includes.Length > 0;
        
        if (!hasIncludes && trackChanges)
        {
            return await _dbSet.FindAsync(id);
        }
        
        IQueryable<T> query = _dbSet;

        if (!trackChanges)
            query = query.AsNoTracking();
        
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        
        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync(bool trackChanges = false, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        if (!trackChanges)
            query = query.AsNoTracking();

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.ToListAsync();
    }
    
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool trackChanges = false, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(predicate);

        if (!trackChanges)
            query = query.AsNoTracking();

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.ToListAsync();
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(v => v.Id == id, cancellationToken);
    }

    public virtual async Task<T?> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        var res = await _context.SaveChangesAsync();
        if (res > 0) return entity;
        return null;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        await _dbSet.Where(op => op.Id == id).ExecuteDeleteAsync();
    }
}