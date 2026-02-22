using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class SqlCustomerRepository(BaseSqlContext sqlContext) : ICustomerRepository
{
    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await sqlContext.Customers
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await sqlContext.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> AddAsync(Customer customer)
    {
        await sqlContext.Customers.AddAsync(customer);
        var res = await sqlContext.SaveChangesAsync();
        if (res > 0) return customer;
        return null;
    }

    public async Task UpdateAsync(Customer customer)
    {
        await sqlContext.Customers.Where(c => c.Id == customer.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(c => c.PartnerId, customer.PartnerId)
            .SetProperty(c => c.AccountName, customer.AccountName)
            .SetProperty(c => c.Email, customer.Email)
            .SetProperty(c => c.LegalName, customer.LegalName)
            .SetProperty(c => c.City, customer.City)
            .SetProperty(c => c.Region, customer.Region)
            .SetProperty(c => c.ZipCode, customer.ZipCode)
            .SetProperty(c => c.CountryCode, customer.CountryCode)
        );
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await sqlContext.Customers.FindAsync(id);
        if (customer != null)
        {
            sqlContext.Customers.Remove(customer);
            await sqlContext.SaveChangesAsync();
        }
    }
}