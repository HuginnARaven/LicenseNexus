using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class ProductTypeService(IProductTypeRepository productTypeRepository) : IProductTypeService
{
    public async Task<IEnumerable<ProductTypeResponseDto>> GetAllProductTypes()
    {
        var categories = await productTypeRepository.GetAllAsync();
        return categories.Select(um => new ProductTypeResponseDto
        {
            Id = um.Id,
            TypeName = um.TypeName,
        });
    }

    public async Task<ProductTypeResponseDto?> GetProductTypeById(int id)
    {
        var productType = await productTypeRepository.GetByIdAsync(id);
        if (productType == null)
        {
            return null;
        }

        return new ProductTypeResponseDto
        {
            Id = productType.Id,
            TypeName = productType.TypeName,
        };
    }

    public async Task AddProductType(ProductTypeRequestDto productTypeDto)
    {
        var productType = new ProductType
        {
            TypeName = productTypeDto.TypeName,
        };

        await productTypeRepository.AddAsync(productType);
    }
}