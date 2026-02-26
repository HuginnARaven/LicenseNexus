using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IProductTypeRepository
{
    Task<IEnumerable<ProductType>> GetAllAsync();
    Task<ProductType?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<ProductType?> AddAsync(ProductType productType);
    Task UpdateAsync(ProductType productType);
    Task DeleteAsync(int id);
}