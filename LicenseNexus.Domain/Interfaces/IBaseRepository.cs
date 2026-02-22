using System.Linq.Expressions;

namespace LicenseNexus.Domain.Interfaces;

public interface IBaseRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(int id, bool trackChanges = false, params Expression<Func<T, object>>[] includes);
    
    Task<IEnumerable<T>> GetAllAsync(bool trackChanges = false, params Expression<Func<T, object>>[] includes);
    
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool trackChanges = false, params Expression<Func<T, object>>[] includes);
    
    Task<T?> AddAsync(T entity);
    
    Task UpdateAsync(T entity);
    
    Task DeleteAsync(int id);
}