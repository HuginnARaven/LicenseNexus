using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;

namespace LicenseNexus.Application.Services;

public class ProductService(
    IProductRepository productRepository, 
    IVendorRepository vendorRepository, 
    IProductGroupRepository productGroupRepository
    ): IProductService
{
    public async Task<ProductModel?> GetByIdAsync(int id)
    {
        return await productRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<ProductModel>> GetAllAsync()
    {
        return await productRepository.GetAllAsync();
    }

    public Task AddAsync(ProductRequestDTO product)
    {
        
        throw new NotImplementedException();
    }

    public Task UpdateAsync(ProductRequestDTO product)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}