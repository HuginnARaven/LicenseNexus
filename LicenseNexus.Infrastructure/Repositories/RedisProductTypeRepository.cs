using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisProductTypeRepository: IProductTypeRepository
{
    private ExtendedSqlContext _context;
    private readonly IDatabase _redisDb;
    
    public RedisProductTypeRepository(ExtendedSqlContext context, RedisContext redisContext)
    {
        _context = context;
        _redisDb = redisContext.Database;
    }

    public async Task<IEnumerable<ProductType>> GetAllAsync()
    {
        return await _context.ProductTypes.ToListAsync();
    }

    public async Task<ProductType?> GetByIdAsync(int id)
    {
        var negativeCacheKey = $"product_type:{id}:notfound";
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return null;

        var productType = await _redisDb.JSON().GetAsync<ProductType>($"product_type:{id}");
        if (productType != null) 
            return productType;
        
        var dbProductType = await _context.ProductTypes.FirstOrDefaultAsync(pt => pt.Id == id);
        if (dbProductType == null)
        {
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));
            return null;
        }
        await _redisDb.JSON().SetAsync($"product_type:{id}", "$", dbProductType);
        return dbProductType;
    }
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var redisKey = $"product_type:{id}";
        var negativeCacheKey = $"product_type:{id}:notfound";
        
        if (await _redisDb.KeyExistsAsync(redisKey))
            return true;
        
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return false;
        
        var existsInDb = await _context.ProductTypes.AnyAsync(pt => pt.Id == id, cancellationToken);

        if (!existsInDb)
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));
        
        return existsInDb;
    }

    public async Task<ProductType?> AddAsync(ProductType productType)
    {
        _context.ProductTypes.Add(productType);
        var res = await _context.SaveChangesAsync();
        
        if (res <= 0)
            return null;
        
        await _redisDb.JSON().SetAsync($"product_type:{productType.Id}", "$", productType);
        return productType;
    }

    public async Task UpdateAsync(ProductType productType)
    {
        _context.ProductTypes.Update(productType);
        await _context.SaveChangesAsync();
        await _redisDb.JSON().SetAsync($"product_type:{productType.Id}", "$", productType);
    }
    
    public async Task DeleteAsync(int id)
    {
        var productType = await _context.ProductTypes.FindAsync(id);
        if (productType == null)
            throw new InvalidOperationException("Object Not Found");
        _context.Remove(productType);
        await  _context.SaveChangesAsync();
        await _redisDb.KeyDeleteAsync($"product_type:{id}");
    }
}