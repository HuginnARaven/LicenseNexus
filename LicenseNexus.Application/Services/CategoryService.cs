using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryResponseDTO>> GetAllCategories()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => new CategoryResponseDTO
        {
            Id = c.Id,
            CategoryName = c.CategoryName,
            IsActive = c.IsActive,
            Description = c.Description,
            CreatedDate = c.CreatedDate,
            Author = c.Author
        });
    }

    public async Task<CategoryResponseDTO?> GetCategoryById(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return null;
        }

        return new CategoryResponseDTO
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            IsActive = category.IsActive,
            Description = category.Description,
            CreatedDate = category.CreatedDate,
            Author = category.Author
        };
    }

    public async Task AddCategory(CategoryRequestDTO categoryDto)
    {
        var category = new Category
        {
            CategoryName = categoryDto.CategoryName,
            IsActive = categoryDto.IsActive,
            Description = categoryDto.Description,
            Author = categoryDto.Author,
            CreatedDate = DateTime.UtcNow
        };

        await _categoryRepository.AddAsync(category);
    }
}