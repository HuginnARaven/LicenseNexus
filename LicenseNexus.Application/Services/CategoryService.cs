using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Exceptions;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository, IEventPublisher eventPublisher) : ICategoryService
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
            Author = c.Author,
            CategoryGroups = c.ProductGroups.Select(pg => new ProductGroupResponseDto
            {
                Id = pg.Id,
                Name = pg.Name,
                IsActive = pg.IsActive,
                Note = pg.Note,
                CreatedDate = pg.CreatedDate,
                Author = pg.Author,
                CategoryId = pg.CategoryId
            }).ToList()
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
            Author = category.Author,
            CategoryGroups = category.ProductGroups.Select(pg => new ProductGroupResponseDto
            {
                Id = pg.Id,
                Name = pg.Name,
                IsActive = pg.IsActive,
                Note = pg.Note,
                CreatedDate = pg.CreatedDate,
                Author = pg.Author,
                CategoryId = pg.CategoryId
            }).ToList()
        };
    }

    public async Task<CategoryResponseDto?> AddCategory(CategoryRequestDto categoryDto)
    {
        var category = new Category
        {
            CategoryName = categoryDto.CategoryName,
            IsActive = categoryDto.IsActive,
            Description = categoryDto.Description,
            Author = categoryDto.Author,
        };

        var result = await categoryRepository.AddAsync(category);
        return result == null ? null : new CategoryResponseDto
        {
            Id = result.Id,
            CategoryName = result.CategoryName,
            IsActive = result.IsActive,
            Description = result.Description,
            CreatedDate = result.CreatedDate,
            Author = result.Author,
            CategoryGroups = new List<ProductGroupResponseDto>()
        };
    }

    public async Task UpdateCategory(int id, CategoryRequestDto category)
    {
        if(!await categoryRepository.ExistsAsync(id))
            throw new NotFoundException($"Category with ID {id} not found");
        
        var updatedCategory = new Category
        {
            Id = id,
            CategoryName = category.CategoryName,
            IsActive = category.IsActive,
            Description = category.Description,
            Author = category.Author,
        };

        await categoryRepository.UpdateAsync(updatedCategory);
        await eventPublisher.PublishAsync(new CategoryUpdatedEvent(updatedCategory));
    }

    public async Task DeleteCategory(int id)
    {
        await categoryRepository.DeleteAsync(id);
    }
}