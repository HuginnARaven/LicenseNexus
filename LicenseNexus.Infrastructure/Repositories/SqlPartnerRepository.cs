using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class SqlPartnerRepository(BaseSqlContext context): BaseSqlRepository<Partner>(context), IPartnerRepository
{
    public override async Task UpdateAsync(Partner partner)
    {
        await _context.Partners.Where(p => p.Id == partner.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.CountryCode, partner.CountryCode)
            .SetProperty(p => p.FullCompanyName, partner.FullCompanyName)
            .SetProperty(p => p.RegistrationNumber, partner.RegistrationNumber)
            .SetProperty(p => p.TaxNumber, partner.TaxNumber)
            .SetProperty(p => p.BankAccountNumber, partner.BankAccountNumber)
            .SetProperty(p => p.BankName, partner.BankName)
            .SetProperty(p => p.Phone, partner.Phone)
            .SetProperty(p => p.Author, partner.Author)
        );
    }

    public Task<bool> AddressExistsAsync(int id)
    {
        return _context.PartnerAddresses.AnyAsync(pa => pa.Id == id);
    }

    public async Task<PartnerAddress?> AddAddressAsync(PartnerAddress address)
    {
        await _context.AddAsync(address);
        var res = await _context.SaveChangesAsync();
        if (res > 0)
            return address;
        return null;
    }

    public async Task EditAddressAsync(PartnerAddress address)
    {
        _context.Update(address);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAddressAsync(int addressId)
    {
        await _context.PartnerAddresses.Where(pa => pa.Id == addressId).ExecuteDeleteAsync();
    }
}