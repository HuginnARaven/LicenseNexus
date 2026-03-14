using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisCurrencyRepository: ICurrencyRepository
{
    private ExtendedSqlContext _context;
    private readonly IDatabase _redisDb;
    
    public RedisCurrencyRepository(ExtendedSqlContext context, RedisContext redisContext)
    {
        _context = context;
        _redisDb = redisContext.Database;
    }
    
    public async Task<IEnumerable<Currency>> GetAllAsync()
    {
        return await _context.Currencies.Where(_ => true).ToListAsync();
    }

    public async Task<Currency?> GetByIdAsync(int id)
    {
        var negativeCacheKey = $"currency:{id}:notfound";
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return null;
        
        var currency = await _redisDb.JSON().GetAsync<Currency>($"currency:{id}");
        if (currency != null) 
            return currency;
        
        var dbCurrency = await _context.Currencies.FirstOrDefaultAsync(c => c.Id == id);
        if (dbCurrency == null)
        {
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));
            return null;
        }
        await _redisDb.JSON().SetAsync($"currency:{id}", "$", dbCurrency);
        return dbCurrency;
    }
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var redisKey = $"currency:{id}";
        var negativeCacheKey = $"currency:{id}:notfound";
        
        if (await _redisDb.KeyExistsAsync(redisKey))
            return true;
        
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return false;
        
        var existsInDb = await _context.Currencies.AnyAsync(c => c.Id == id, cancellationToken);

        if (!existsInDb)
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));
        
        return existsInDb;
    }

    public async Task<Currency?> AddAsync(Currency currency)
    {
        _context.Currencies.Add(currency);
        var res = await _context.SaveChangesAsync();
        if (res <= 0) 
            return null;
        await _redisDb.JSON().SetAsync($"currency:{currency.Id}", "$", currency);
        return currency;
    }

    public async Task UpdateAsync(Currency currency)
    {
        _context.Currencies.Update(currency);
        await _context.SaveChangesAsync();
        await _redisDb.JSON().SetAsync($"currency:{currency.Id}", "$", currency);
    }

    public async Task DeleteAsync(int id)
    {
        var currency = await _context.Currencies.FindAsync(id);
        if (currency == null)
            throw new InvalidOperationException("Object Not Found");
        _context.Remove(currency);
        await  _context.SaveChangesAsync();
        await _redisDb.KeyDeleteAsync($"currency:{id}");
    }
}