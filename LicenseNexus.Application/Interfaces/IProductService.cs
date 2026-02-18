using LicenseNexus.Application.DTOs;
using LicenseNexus.Domain.Models;

namespace LicenseNexus.Application.Interfaces;

public interface IProductService
{
    Task<ProductModel?> GetByIdAsync(int id);
    Task<IEnumerable<ProductModel>> GetAllAsync();
    Task AddAsync(ProductRequestDTO product);
    Task UpdateAsync(int id, ProductRequestDTO product);
    Task DeleteAsync(int id);
}