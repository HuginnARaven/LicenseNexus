using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDTO>> GetAllCategories();
    Task<CategoryResponseDTO?> GetCategoryById(int id);
    Task AddCategory(CategoryRequestDTO category);
}