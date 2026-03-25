using FluentValidation;
using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Exceptions;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class ProductGroupService(
    IProductGroupRepository productGroupRepository, 
    IEventPublisher eventPublisher, IValidator<ProductGroupRequestDto> createValidator, 
    IValidator<ProductGroupEditRequestDto> editValidator
    ) : IProductGroupService
{
    public async Task<IEnumerable<ProductGroupResponseDto>> GetAllProductGroups()
    {
        var groups = await productGroupRepository.GetAllAsync();
        return groups.Select(g => new ProductGroupResponseDto
        {
            Id = g.Id,
            Name = g.Name,
            IsActive = g.IsActive,
            Note = g.Note,
            CreatedDate = g.CreatedDate,
            Author = g.Author,
            CategoryId = g.CategoryId
        });
    }

    public async Task<ProductGroupResponseDto?> GetProductGroupById(int id)
    {
        var group = await productGroupRepository.GetByIdAsync(id);
        if (group == null)
        {
            return null;
        }

        return new ProductGroupResponseDto
        {
            Id = group.Id,
            Name = group.Name,
            IsActive = group.IsActive,
            Note = group.Note,
            CreatedDate = group.CreatedDate,
            Author = group.Author,
            CategoryId = group.CategoryId
        };
    }

    public async Task<ProductGroupResponseDto?> AddProductGroup(ProductGroupRequestDto groupDto)
    {
        await createValidator.ValidateAndThrowAsync(groupDto);
        var group = new ProductGroup
        {
            Name = groupDto.Name,
            IsActive = groupDto.IsActive,
            Note = groupDto.Note,
            Author = groupDto.Author,
            CategoryId = groupDto.CategoryId,
            CreatedDate = DateTime.UtcNow
        };
        var result = await productGroupRepository.AddAsync(group);
        return result == null ? null : new ProductGroupResponseDto
        {
            Id = result.Id,
            Name = result.Name,
            IsActive = result.IsActive,
            Note = result.Note,
            CreatedDate = result.CreatedDate,
            Author = result.Author,
            CategoryId = result.CategoryId
        };
    }

    public async Task UpdateProductGroup(int id, ProductGroupEditRequestDto productGroup)
    {
        if (!await productGroupRepository.ExistsAsync(id))
            throw new NotFoundException($"Product group with ID {id} not found");
        await editValidator.ValidateAndThrowAsync(productGroup);
        var group = new ProductGroup
        {
            Id = id,
            Name = productGroup.Name,
            IsActive = productGroup.IsActive,
            Note = productGroup.Note,
            Author = productGroup.Author
        };

        await productGroupRepository.UpdateAsync(group);
        await eventPublisher.PublishAsync(new GroupUpdatedEvent(group));
    }
    
    public async Task DeleteProductGroup(int id)
    {
        if (!await productGroupRepository.ExistsAsync(id))
            throw new NotFoundException($"Product group with ID {id} not found");

        await productGroupRepository.DeleteAsync(id);
    }
}