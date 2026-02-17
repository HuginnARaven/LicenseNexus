using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> GetAllCategories();
    Task<CategoryResponseDto?> GetCategoryById(int id);
    Task AddCategory(CategoryRequestDto category);
}