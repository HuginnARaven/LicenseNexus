using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Models;

namespace LicenseNexus.Domain.Interfaces;

public interface IProductRepository
{
    Task<ProductModel?> GetByIdAsync(int id);
    Task<IEnumerable<ProductModel>> GetAllAsync();
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<ProductModel>> GetPaginatedAsync(
        int page, int pageSize, 
        int? categoryId, int? groupId, 
        int? vendorId, string? search,
        double? priceFrom, double? priceTo);
    Task<ProductModel?> AddAsync(ProductModel product);
    Task UpdateAsync(ProductModel product);
    Task PatchAsync(int id, ProductPatchFields updates);
    Task DeleteAsync(int id);
    
    Task<ProductPrice?> GetPriceAsync(int productId, int priceId);
    Task<bool> ExistsPriceAsync(long priceId, long productId, CancellationToken cancellationToken = default);
    Task<ProductPrice?> AddPrice(ProductPrice price);
    Task UpdatePrice(ProductPrice price);
    Task DeletePrice(int productId, int priceId);
    
    Task AddTag(int productId, int tagId);
    Task DeleteTag(int productId, int tagId);
}