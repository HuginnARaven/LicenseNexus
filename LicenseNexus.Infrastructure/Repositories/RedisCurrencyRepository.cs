using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisCurrencyRepository: ICurrencyRepository
{
    private ExtendedSqlContext _context;
    
    public RedisCurrencyRepository(ExtendedSqlContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Currency>> GetAllAsync()
    {
        return await _context.Currencies.Where(_ => true).ToListAsync();
    }

    public async Task<Currency?> GetByIdAsync(int id)
    {
        return await _context.Currencies.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Currency currency)
    {
        _context.Currencies.Add(currency);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Currency currency)
    {
        _context.Currencies.Update(currency);
        await _context.SaveChangesAsync();
    }
}