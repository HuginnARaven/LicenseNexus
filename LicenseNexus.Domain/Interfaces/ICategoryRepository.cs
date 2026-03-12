using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    
    Task<Category?> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
}
