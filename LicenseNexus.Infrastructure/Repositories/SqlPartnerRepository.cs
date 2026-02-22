using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class SqlPartnerRepository(BaseSqlContext sqlContext): IPartnerRepository
{
    public async Task<IEnumerable<Partner>> GetAllAsync()
    {
        return await sqlContext.Partners.ToListAsync();
    }

    public async Task<Partner?> GetByIdAsync(int id)
    {
        return await sqlContext.Partners
            .FindAsync(id);
    }

    public async Task<Partner?> AddAsync(Partner partner)
    {
        await sqlContext.Partners.AddAsync(partner);
        var res = await sqlContext.SaveChangesAsync();
        if (res > 0) return partner;
        return null;
    }

    public async Task UpdateAsync(Partner partner)
    {
        await sqlContext.Partners.Where(p => p.Id == partner.Id).ExecuteUpdateAsync(setters => setters
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

    public async Task DeleteAsync(int id)
    {
        var partner = await sqlContext.Partners.FindAsync(id);
        if (partner != null)
        {
            sqlContext.Partners.Remove(partner);
            await sqlContext.SaveChangesAsync();
        }
    }
}