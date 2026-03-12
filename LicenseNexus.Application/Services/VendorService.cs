using FluentValidation;
using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Exceptions;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class VendorService(
    IVendorRepository vendorRepository, 
    IEventPublisher eventPublisher,
    IValidator<VendorRequestDTO> validator
    ) : IVendorService
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

    public async Task<VendorResponceDTO?> AddVendor(VendorRequestDTO vendor)
    {
        await validator.ValidateAndThrowAsync(vendor);
        var newVendor = new Vendor
        {
            Name = vendor.Name,
            OriginalName = vendor.OriginalName,
            Description = vendor.Description,
            CountryCode = vendor.CountryCode,
            Logo = vendor.Logo ?? ""
        };
        var resVendor = await vendorRepository.AddAsync(newVendor);
        return resVendor == null ? null : MapModelToDto(resVendor);
    }

    public async Task UpdateVendor(int id, VendorRequestDTO vendor)
    {
        await validator.ValidateAndThrowAsync(vendor);
        if (!await vendorRepository.ExistsAsync(id))
            throw new NotFoundException($"Vendor with ID {id} not found");
        
        var newVendor = new Vendor
        {
            Id = id,
            Name = vendor.Name,
            OriginalName = vendor.OriginalName,
            Description = vendor.Description,
            CountryCode = vendor.CountryCode,
            Logo = vendor.Logo
        };
        await vendorRepository.UpdateAsync(newVendor);
        await eventPublisher.PublishAsync(new VendorUpdatedEvent(newVendor));
    }

    public async Task DeleteVendor(int id)
    {
        await vendorRepository.DeleteAsync(id);
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