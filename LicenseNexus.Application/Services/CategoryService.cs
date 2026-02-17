using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<IEnumerable<CategoryResponseDto>> GetAllCategories()
    {
        var categories = await categoryRepository.GetAllAsync();
        return categories.Select(c => new CategoryResponseDto
        {
            Id = c.Id,
            CategoryName = c.CategoryName,
            IsActive = c.IsActive,
            Description = c.Description,
            CreatedDate = c.CreatedDate,
            Author = c.Author
        });
    }

    public async Task<CategoryResponseDto?> GetCategoryById(int id)
    {
        var category = await categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return null;
        }

        return new CategoryResponseDto
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            IsActive = category.IsActive,
            Description = category.Description,
            CreatedDate = category.CreatedDate,
            Author = category.Author
        };
    }

    public async Task AddCategory(CategoryRequestDto categoryDto)
    {
        var category = new Category
        {
            CategoryName = categoryDto.CategoryName,
            IsActive = categoryDto.IsActive,
            Description = categoryDto.Description,
            Author = categoryDto.Author,
            CreatedDate = DateTime.UtcNow
        };

        await categoryRepository.AddAsync(category);
    }
}