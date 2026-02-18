using LicenseNexus.Domain.Models;

namespace LicenseNexus.Infrastructure.Services;

public interface IProductAggregatorService
{
    Task AggregateProductAsync(int productId);
    Task CacheProductModelAsync(ProductModel product);
    Task AggregateAllProductsAsync();
    Task BuildIndexesAsync();
}