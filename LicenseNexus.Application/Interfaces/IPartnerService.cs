using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface IPartnerService
{
    Task<IEnumerable<PartnerResponseDto>> GetAllPartnersAsync();
    Task<PartnerResponseDto?> GetPartnerByIdAsync(int id);
    Task<PartnerResponseDto?> CreatePartnerAsync(PartnerRequestDto partner);
    Task UpdatePartnerAsync(int id, PartnerRequestDto partner);
    Task DeletePartnerAsync(int id);
    Task<PartnerAddressResponseDto?> AddAddressAsync(PartnerAddressRequestDto addressDto);
    Task UpdateAddressAsync(int id, PartnerAddressRequestDto addressDto);
    Task DeleteAddressAsync(int id);
}