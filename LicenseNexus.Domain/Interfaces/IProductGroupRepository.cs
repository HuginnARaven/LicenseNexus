using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IProductGroupRepository
{
    Task<IEnumerable<ProductGroup>> GetAllAsync();
    Task<ProductGroup?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<ProductGroup?> AddAsync(ProductGroup group);
    Task UpdateAsync(ProductGroup group);
}