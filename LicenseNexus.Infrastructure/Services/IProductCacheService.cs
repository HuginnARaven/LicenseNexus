using LicenseNexus.Domain.Models;

namespace LicenseNexus.Infrastructure.Services;

public interface IProductCacheService
{
    Task<ProductModel?> CacheProductByIdAsync(int productId);
    Task CacheProductModelAsync(ProductModel product);
    Task RemoveProductCacheAsync(int productId);
    Task CacheAllProductsAsync();
}