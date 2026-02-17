using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class VendorService(IVendorRepository vendorRepository) : IVendorService
{
    public async Task<IEnumerable<VendorResponceDTO>> GetAllVendors()
    {
        var vendors = await vendorRepository.GetAllAsync();
        var result = new List<VendorResponceDTO>();
        foreach (var vendor in vendors)
        {
            result.Add(MapModelToDto(vendor));
        }
        return result;
    }

    public async Task<VendorResponceDTO?> GetVendorById(int id)
    {
        var vendor = await vendorRepository.GetByIdAsync(id);
        return vendor == null ? null : MapModelToDto(vendor);
    }

    public async Task AddVendor(VendorRequestDTO vendor)
    {
        var newVendor = new Vendor
        {
            Name = vendor.Name,
            OriginalName = vendor.OriginalName,
            Description = vendor.Description,
            CountryCode = vendor.CountryCode,
            Logo = vendor.Logo ?? ""
        };
        await vendorRepository.AddAsync(newVendor);
    }

    private VendorResponceDTO MapModelToDto(Vendor vendor)
    {
        return new VendorResponceDTO
        {
            Id = vendor.Id,
            Name = vendor.Name,
            OriginalName = vendor.OriginalName,
            Description = vendor.Description,
            CountryCode = vendor.CountryCode,
            Logo = vendor.Logo
        };
    }
}