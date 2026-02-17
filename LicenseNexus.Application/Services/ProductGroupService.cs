using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class ProductGroupService : IProductGroupService
{
    private readonly IProductGroupRepository _productGroupRepository;

    public ProductGroupService(IProductGroupRepository productGroupRepository)
    {
        _productGroupRepository = productGroupRepository;
    }

    public async Task<IEnumerable<ProductGroupResponseDTO>> GetAllProductGroups()
    {
        var groups = await _productGroupRepository.GetAllAsync();
        return groups.Select(g => new ProductGroupResponseDTO
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

    public async Task<ProductGroupResponseDTO?> GetProductGroupById(int id)
    {
        var group = await _productGroupRepository.GetByIdAsync(id);
        if (group == null)
        {
            return null;
        }

        return new ProductGroupResponseDTO
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

    public async Task AddProductGroup(ProductGroupRequestDTO groupDto)
    {
        var group = new ProductGroup
        {
            Name = groupDto.Name,
            IsActive = groupDto.IsActive,
            Note = groupDto.Note,
            Author = groupDto.Author,
            CategoryId = groupDto.CategoryId,
            CreatedDate = DateTime.UtcNow
        };

        await _productGroupRepository.AddAsync(group);
    }
}