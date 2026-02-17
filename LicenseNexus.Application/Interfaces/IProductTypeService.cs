using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface IProductTypeService
{
    Task<IEnumerable<ProductTypeResponseDto>> GetAllProductTypes();
    Task<ProductTypeResponseDto?> GetProductTypeById(int id);
    Task AddProductType(ProductTypeRequestDto productTypeDto);
}