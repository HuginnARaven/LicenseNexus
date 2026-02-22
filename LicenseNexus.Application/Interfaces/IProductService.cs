using LicenseNexus.Application.DTOs;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Models;

namespace LicenseNexus.Application.Interfaces;

public interface IProductService
{
    Task<ProductModel?> GetByIdAsync(int id);
    Task<IEnumerable<ProductModel>> GetAllAsync();
    Task<PaginatedResult<ProductModel>> GetPaginatedAsync(ProductFilterDto filter);
    Task<ProductModel?> AddAsync(ProductRequestDTO product);
    Task UpdateAsync(int id, ProductRequestDTO product);
    Task PatchAsync(int id, ProductPatchFields updates);
    Task DeleteAsync(int id);
    Task<ProductPrice?> AddProductPrice(int productId, ProductPriceRequestDto price);
    Task UpdateProductPrice(int productId, int priceId, ProductPriceRequestDto price);
    Task DeleteProductPrice(int productId, int priceId);
}