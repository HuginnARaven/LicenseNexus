using LicenseNexus.Application.DTOs;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Models;

namespace LicenseNexus.Application.Interfaces;

public interface IProductService
{
    Task<ProductModel?> GetByIdAsync(int id);
    Task<IEnumerable<ProductModel>> GetAllAsync();
    Task<PaginatedResult<ProductModel>> GetPaginatedAsync(ProductFilterDto filter);
    Task<ProductModel?> AddAsync(ProductRequestDto product);
    Task UpdateAsync(int id, ProductRequestDto product);
    Task PatchAsync(int id, ProductPatchFieldsDto updates);
    Task DeleteAsync(int id);
    Task<ProductPrice?> AddProductPrice(int productId, ProductPriceRequestDto price);
    Task UpdateProductPrice(int productId, int priceId, ProductPriceRequestDto price);
    Task DeleteProductPrice(int productId, int priceId);
    Task AddProductTag(int productId, int tagId);
    Task DeleteProductTag(int productId, int tagId);
}