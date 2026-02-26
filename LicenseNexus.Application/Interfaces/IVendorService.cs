using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface IVendorService
{
    Task<IEnumerable<VendorResponceDTO>> GetAllVendors();
    Task<VendorResponceDTO?> GetVendorById(int id);
    Task<VendorResponceDTO?> AddVendor(VendorRequestDTO vendor);
    Task UpdateVendor(int id, VendorRequestDTO vendor);
    Task DeleteVendor(int id);
}