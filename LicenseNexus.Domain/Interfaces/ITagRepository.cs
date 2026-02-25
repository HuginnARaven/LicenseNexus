using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface ITagRepository
{
    Task<IEnumerable<Tag>> GetAllAsync();
    Task<Tag?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<Tag?> AddAsync(Tag tag);
    Task UpdateAsync(Tag tag);
    Task DeleteAsync(int id);
}