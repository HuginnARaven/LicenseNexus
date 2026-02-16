namespace LicenseNexus.Infrastructure.Services;

public interface IProductAggregatorService
{
    Task AggregateProductAsync(int productId);
    Task AggregateAllProductsAsync();
    Task BuildIndexesAsync();
}