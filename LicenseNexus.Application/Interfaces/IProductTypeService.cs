using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface IProductTypeService
{
    Task<IEnumerable<ProductTypeResponseDto>> GetAllProductTypes();
    Task<ProductTypeResponseDto?> GetProductTypeById(int id);
    Task<ProductTypeResponseDto?> AddProductType(ProductTypeRequestDto productTypeDto);
    Task UpdateProductType(int id, ProductTypeRequestDto productTypeDto);
    Task DeleteProductType(int id);
}