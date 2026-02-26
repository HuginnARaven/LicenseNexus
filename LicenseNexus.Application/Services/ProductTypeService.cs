using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class ProductTypeService(IProductTypeRepository productTypeRepository, IEventPublisher eventPublisher) : IProductTypeService
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

    public async Task<ProductTypeResponseDto?> AddProductType(ProductTypeRequestDto productTypeDto)
    {
        var productType = new ProductType
        {
            TypeName = productTypeDto.TypeName,
        };

        var result = await productTypeRepository.AddAsync(productType);
        return result == null ? null : new ProductTypeResponseDto
        {
            Id = result.Id,
            TypeName = result.TypeName
        };
    }

    public async Task UpdateProductType(int id, ProductTypeRequestDto productTypeDto)
    {
        var productType = new ProductType
        {
            Id = id,
            TypeName = productTypeDto.TypeName,
        };

        await productTypeRepository.UpdateAsync(productType);
        await eventPublisher.PublishAsync(new ProductTypeUpdatedEvent(productType));
    }
    
    public async Task DeleteProductType(int id)
    {
        await productTypeRepository.DeleteAsync(id);
    }
}