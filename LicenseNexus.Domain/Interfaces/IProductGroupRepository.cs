using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IProductGroupRepository
{
    Task<IEnumerable<ProductGroup>> GetAllAsync();
    Task<ProductGroup?> GetByIdAsync(int id);
    Task AddAsync(ProductGroup group);
    Task UpdateAsync(ProductGroup group);
}