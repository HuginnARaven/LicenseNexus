using LicenseNexus.Application.DTOs;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Application.Interfaces;

public interface IPartnerService
{
    Task<IEnumerable<Partner>> GetAllAsync();
    Task<Partner?> GetByIdAsync(int id);
    Task<Partner?> CreateAsync(PartnerRequestDto partner);
    Task UpdateAsync(int id, PartnerRequestDto partner);
    Task DeleteAsync(int id);
}