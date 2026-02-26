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
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Currencies.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Currency?> AddAsync(Currency currency)
    {
        _context.Currencies.Add(currency);
        var res = await _context.SaveChangesAsync();
        if (res > 0)
            return currency;
        
        return null;
    }

    public async Task UpdateAsync(Currency currency)
    {
        _context.Currencies.Update(currency);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var currency = await _context.Currencies.FindAsync(id);
        if (currency == null)
            throw new InvalidOperationException("Object Not Found");
        _context.Remove(currency);
        await  _context.SaveChangesAsync();
    }
}