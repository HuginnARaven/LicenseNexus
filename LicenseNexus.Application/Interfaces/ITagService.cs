using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface ITagService
{
    Task<IEnumerable<TagResponseDto>> GetAllTags();
    Task<TagResponseDto?> GetTagById(int id);
    Task<TagResponseDto?> AddTag(TagRequestDto tag);
    Task UpdateTag(int id, TagRequestDto tad);
    Task DeleteTag(int id);
}