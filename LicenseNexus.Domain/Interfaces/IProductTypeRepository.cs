using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IProductTypeRepository
{
    Task<IEnumerable<ProductType>> GetAllAsync();
    Task<ProductType?> GetByIdAsync(int id);
    Task AddAsync(ProductType productType);
    Task UpdateAsync(ProductType productType);
    //TODO: mb add Update/Delete 
}