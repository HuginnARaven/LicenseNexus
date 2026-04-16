using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IPartnerRepository: IBaseRepository<Partner>
{
    Task<bool> AddressExistsAsync(int id);
    Task<PartnerAddress?> AddAddressAsync(PartnerAddress address);
    Task EditAddressAsync(PartnerAddress address);
    Task DeleteAddressAsync(int addressId);
}