using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class TagService(ITagRepository repository, IEventPublisher eventPublisher): ITagService
{
    public async Task<IEnumerable<TagResponseDto>> GetAllTags()
    {
        var tags = await repository.GetAllAsync();
        return tags.Select(t => new TagResponseDto
        {
            Id = t.Id,
            Name = t.Name,
        });
    }

    public async Task<TagResponseDto?> GetTagById(int id)
    {
        var tag = await repository.GetByIdAsync(id);
        if (tag == null)
        {
            return null;
        }

        return new TagResponseDto()
        {
            Id = tag.Id,
            Name = tag.Name,
        };
    }

    public async Task<TagResponseDto?> AddTag(TagRequestDto tagDto)
    {
        var tag = new Tag
        {
            Name = tagDto.Name
        };
        var tagRes = await repository.AddAsync(tag);
        if (tagRes == null) return null;
        
        return new TagResponseDto()
        {
            Id = tagRes.Id,
            Name = tagRes.Name,
        };
    }

    public async Task UpdateTag(int id, TagRequestDto tagDto)
    {
        var tag = new Tag { Id = id, Name = tagDto.Name };
        await repository.UpdateAsync(tag);
        await eventPublisher.PublishAsync(new TagUpdatedEvent(tag));
    }

    public async Task DeleteTag(int id)
    {
        await repository.DeleteAsync(id);
    }
}