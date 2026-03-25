using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface IProductGroupService
{
    Task<IEnumerable<ProductGroupResponseDto>> GetAllProductGroups();
    Task<ProductGroupResponseDto?> GetProductGroupById(int id);
    Task<ProductGroupResponseDto?> AddProductGroup(ProductGroupRequestDto productGroup);
    Task UpdateProductGroup(int id, ProductGroupEditRequestDto productGroup);
    Task DeleteProductGroup(int id);
}