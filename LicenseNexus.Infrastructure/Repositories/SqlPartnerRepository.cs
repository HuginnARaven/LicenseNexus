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
}