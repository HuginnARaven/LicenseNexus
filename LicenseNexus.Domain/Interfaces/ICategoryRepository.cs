using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    //TODO: mb add Update/Delete 
}