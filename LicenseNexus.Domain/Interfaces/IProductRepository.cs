using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Models;

namespace LicenseNexus.Domain.Interfaces;

public interface IProductRepository
{
    Task<ProductModel?> GetByIdAsync(int id);
    Task<IEnumerable<ProductModel>> GetAllAsync();
    Task<PaginatedResult<ProductModel>> GetPaginatedAsync(
        int page, int pageSize, 
        int? categoryId, int? groupId, 
        int? vendorId, string? search,
        double? priceFrom, double? priceTo);
    Task<ProductModel?> AddAsync(ProductModel product);
    Task UpdateAsync(ProductModel product);
    Task PatchAsync(int id, ProductPatchFields updates);
    Task DeleteAsync(int id);
}