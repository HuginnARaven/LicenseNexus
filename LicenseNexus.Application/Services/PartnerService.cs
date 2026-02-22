using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class PartnerService(IPartnerRepository repository) : IPartnerService
{
    public async Task<IEnumerable<Partner>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task<Partner?> GetByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<Partner?> CreateAsync(PartnerRequestDto partnerDto)
    {
        var partner = new Partner
        {
            Status = "Created",
            CountryCode = partnerDto.CountryCode,
            FullCompanyName = partnerDto.FullCompanyName,
            RegistrationNumber = partnerDto.RegistrationNumber,
            TaxNumber = partnerDto.TaxNumber,
            BankAccountNumber = partnerDto.BankAccountNumber,
            BankName = partnerDto.BankName,
            Phone = partnerDto.Phone,
            Author = partnerDto.Author
        };

        return await repository.AddAsync(partner);
    }

    public async Task UpdateAsync(int id, PartnerRequestDto partnerDto)
    {
        var partner = new Partner
        {
            Id = id,
            CountryCode = partnerDto.CountryCode,
            FullCompanyName = partnerDto.FullCompanyName,
            RegistrationNumber = partnerDto.RegistrationNumber,
            TaxNumber = partnerDto.TaxNumber,
            BankAccountNumber = partnerDto.BankAccountNumber,
            BankName = partnerDto.BankName,
            Phone = partnerDto.Phone,
            Author = partnerDto.Author
        };
        await repository.UpdateAsync(partner);
    }

    public async Task DeleteAsync(int id)
    {
        await repository.DeleteAsync(id);
    }
}