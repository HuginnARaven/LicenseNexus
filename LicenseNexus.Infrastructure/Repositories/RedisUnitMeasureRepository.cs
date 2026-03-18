using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisUnitMeasureRepository: IUnitMeasureRepository
{
    private ExtendedSqlContext _context;
    private readonly IDatabase _redisDb;
    
    public RedisUnitMeasureRepository(ExtendedSqlContext context, RedisContext redisContext)
    {
        _context = context;
        _redisDb = redisContext.Database;
    }
    
    public async Task<IEnumerable<UnitMeasure>> GetAllAsync()
    {
        return await _context.UnitMeasures.Where(_ => true).ToListAsync();
    }

    public async Task<UnitMeasure?> GetByIdAsync(int id)
    {
        var negativeCacheKey = $"unit_measure:{id}:notfound";
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return null;
        
        var unitMeasure = await _redisDb.JSON().GetAsync<UnitMeasure>($"unit_measure:{id}");
        if (unitMeasure != null)
            return unitMeasure;
        
        var dbUnitMeasure = await _context.UnitMeasures.FirstOrDefaultAsync(um => um.Id == id);
        if (dbUnitMeasure == null)
        {
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));
            return null;
        }
        await _redisDb.JSON().SetAsync($"unit_measure:{id}", "$", dbUnitMeasure);
        return dbUnitMeasure;
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var redisKey = $"unit_measure:{id}";
        var negativeCacheKey = $"unit_measure:{id}:notfound";
        
        if (await _redisDb.KeyExistsAsync(redisKey))
            return true;
        
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return false;
        
        var existsInDb = await _context.UnitMeasures.AnyAsync(um => um.Id == id, cancellationToken);

        if (!existsInDb)
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));
        
        return existsInDb;
    }
    
    public async Task<UnitMeasure?> AddAsync(UnitMeasure unitMeasure)
    {
        _context.UnitMeasures.Add(unitMeasure);
        var res = await _context.SaveChangesAsync();
        
        if (res <= 0)
            return null;
        
        await _redisDb.JSON().SetAsync($"unit_measure:{unitMeasure.Id}", "$", unitMeasure);
        return unitMeasure;
    }

    public async Task UpdateAsync(UnitMeasure unitMeasure)
    {
        _context.UnitMeasures.Update(unitMeasure);
        await _context.SaveChangesAsync();
        await _redisDb.JSON().SetAsync($"unit_measure:{unitMeasure.Id}", "$", unitMeasure);
    }
    
    public async Task DeleteAsync(int id)
    {
        // var unitMeasure = await _context.UnitMeasures.FindAsync(id);
        // if (unitMeasure == null)
        //     throw new InvalidOperationException("Object Not Found");
        var unitMeasure = new UnitMeasure { Id = id };
        _context.UnitMeasures.Attach(unitMeasure);
        _context.UnitMeasures.Remove(unitMeasure);
        await  _context.SaveChangesAsync();
        await _redisDb.KeyDeleteAsync($"unit_measure:{id}");
    }
}