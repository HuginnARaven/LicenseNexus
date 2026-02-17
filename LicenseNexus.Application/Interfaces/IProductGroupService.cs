using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface IProductGroupService
{
    Task<IEnumerable<ProductGroupResponseDTO>> GetAllProductGroups();
    Task<ProductGroupResponseDTO?> GetProductGroupById(int id);
    Task AddProductGroup(ProductGroupRequestDTO productGroup);
}