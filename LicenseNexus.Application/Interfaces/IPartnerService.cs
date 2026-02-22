using LicenseNexus.Application.DTOs;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Application.Interfaces;

public interface IPartnerService
{
    Task<IEnumerable<PartnerResponseDto>> GetAllAsync();
    Task<PartnerResponseDto?> GetByIdAsync(int id);
    Task<PartnerResponseDto?> CreateAsync(PartnerRequestDto partner);
    Task UpdateAsync(int id, PartnerRequestDto partner);
    Task DeleteAsync(int id);
}