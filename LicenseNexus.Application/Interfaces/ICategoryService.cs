using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> GetAllCategories();
    Task<CategoryResponseDto?> GetCategoryById(int id);
    Task<CategoryResponseDto?> AddCategory(CategoryRequestDto category);
    Task UpdateCategory(int id, CategoryRequestDto category);
    Task DeleteCategory(int id);
}