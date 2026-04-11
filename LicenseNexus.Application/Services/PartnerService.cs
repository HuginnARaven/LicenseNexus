using FluentValidation;
using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Exceptions;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class PartnerService(IPartnerRepository repository, IValidator<PartnerRequestDto> validator) : IPartnerService
{
    public async Task<IEnumerable<PartnerResponseDto>> GetAllPartnersAsync()
    {
        var partners = await repository.GetAllAsync(false, p => p.Addresses, p => p.Customers);
        return partners.Select(MapPartnerToDto);
    }

    public async Task<PartnerResponseDto?> GetPartnerByIdAsync(int id)
    {
        var partner = await repository.GetByIdAsync(id, false, p => p.Addresses, p => p.Customers);
        if (partner == null)
            return null;
        return MapPartnerToDto(partner);
    }

    public async Task<PartnerResponseDto?> CreatePartnerAsync(PartnerRequestDto partnerDto)
    {
        await validator.ValidateAndThrowAsync(partnerDto);
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

        var createdPartner = await repository.AddAsync(partner);
        if (createdPartner == null)
            return null;
        return MapPartnerToDto(createdPartner);
    }

    public async Task UpdatePartnerAsync(int id, PartnerRequestDto partnerDto)
    {
        await validator.ValidateAndThrowAsync(partnerDto);
        if (!await repository.ExistsAsync(id))
            throw new NotFoundException($"Partner with ID {id} not found");
        
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

    public async Task DeletePartnerAsync(int id)
    {
        await repository.DeleteAsync(id);
    }
    
    private PartnerResponseDto MapPartnerToDto(Partner partner)
    {
        return new PartnerResponseDto
        {
            Id = partner.Id,
            Status = partner.Status,
            CountryCode = partner.CountryCode,
            FullCompanyName = partner.FullCompanyName,
            RegistrationNumber = partner.RegistrationNumber,
            TaxNumber = partner.TaxNumber,
            BankAccountNumber = partner.BankAccountNumber,
            BankName = partner.BankName,
            Phone = partner.Phone,
            CreatedDate = partner.CreatedDate,
            Author = partner.Author,
            Addresses = partner.Addresses.Select(a => new PartnerAddressResponseDto
            {
                Id = a.Id,
                PartnerId = a.PartnerId,
                City = a.City,
                AddressFull = a.AddressFull,
                Region = a.Region,
                ZipCode = a.ZipCode
            }).ToList(),
            Customers = partner.Customers.Select(c => new CustomerResponseDto
            {
                Id = c.Id,
                PartnerId = c.PartnerId,
                AccountName = c.AccountName,
                Email = c.Email,
                LegalName = c.LegalName,
                City = c.City,
                Region = c.Region,
                ZipCode = c.ZipCode,
                CountryCode = c.CountryCode
            }).ToList()
        };
    }
}