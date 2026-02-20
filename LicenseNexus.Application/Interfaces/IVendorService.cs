using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface IVendorService
{
    Task<IEnumerable<VendorResponceDTO>> GetAllVendors();
    Task<VendorResponceDTO?> GetVendorById(int id);
    Task AddVendor(VendorRequestDTO vendor);
    Task UpdateVendor(int id, VendorRequestDTO vendor);
}