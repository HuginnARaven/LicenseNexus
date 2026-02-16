using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Models;

namespace LicenseNexus.Domain.Interfaces;

public interface IProductRepository
{
    Task<ProductModel?> GetByIdAsync(int id);
    Task<IEnumerable<ProductModel>> GetAllAsync();
    Task AddAsync(ProductModel product);
    Task UpdateAsync(ProductModel product);
    Task DeleteAsync(int id);
}