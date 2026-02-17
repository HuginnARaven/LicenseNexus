using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface IProductGroupService
{
    Task<IEnumerable<ProductGroupResponseDto>> GetAllProductGroups();
    Task<ProductGroupResponseDto?> GetProductGroupById(int id);
    Task AddProductGroup(ProductGroupRequestDto productGroup);
}