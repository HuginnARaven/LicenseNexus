using LicenseNexus.Domain.Models;

namespace LicenseNexus.Domain.Interfaces;

public interface IProductCacheService
{
    Task<ProductModel?> CacheProductByIdAsync(int productId);
    Task CacheProductModelAsync(ProductModel product);
    Task RemoveProductCacheAsync(int productId);
    Task CacheAllProductsAsync();
}