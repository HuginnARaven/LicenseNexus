using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class SqlCustomerRepository(BaseSqlContext context) : BaseSqlRepository<Customer>(context), ICustomerRepository
{
    public override async Task UpdateAsync(Customer entity)
    {
        await _context.Customers.Where(c => c.Id == entity.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(c => c.PartnerId, entity.PartnerId)
            .SetProperty(c => c.AccountName, entity.AccountName)
            .SetProperty(c => c.Email, entity.Email)
            .SetProperty(c => c.LegalName, entity.LegalName)
            .SetProperty(c => c.City, entity.City)
            .SetProperty(c => c.Region, entity.Region)
            .SetProperty(c => c.ZipCode, entity.ZipCode)
            .SetProperty(c => c.CountryCode, entity.CountryCode)
        );
    }
}