using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IPartnerRepository
{
    Task<IEnumerable<Partner>> GetAllAsync();
    Task<Partner?> GetByIdAsync(int id);
    Task<Partner?> AddAsync(Partner partner);
    Task UpdateAsync(Partner partner);
    Task DeleteAsync(int id);
}