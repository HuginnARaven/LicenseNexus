using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using NRedisStack.RedisStackCommands;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisVendorRepository: IVendorRepository
{
    private ExtendedSqlContext _context;
    private readonly IDatabase _redisDb;
    
    public RedisVendorRepository(ExtendedSqlContext context, RedisContext redisContext)
    {
        _context = context;
        _redisDb = redisContext.Database;
    }
    public async Task<IEnumerable<Vendor>> GetAllAsync()
    {
        return await _context.Vendors.Where(_ => true).ToListAsync();
    }

    public async Task<Vendor?> GetByIdAsync(int id)
    {
        var vendor = await _redisDb.JSON().GetAsync<Vendor>($"vendor:{id}");
        if (vendor == null)
        {            
            var dbVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
            if (dbVendor != null)
            {
                await _redisDb.JSON().SetAsync($"vendor:{id}", "$", dbVendor);
                return dbVendor;
            }
        }
        return vendor;
    }
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        //return await _context.Vendors.AnyAsync(v => v.Id == id, cancellationToken);
        return await _redisDb.KeyExistsAsync($"vendor:{id}");
    }

    public async Task<Vendor?> AddAsync(Vendor vendor)
    {
        _context.Vendors.Add(vendor);
        var res = await _context.SaveChangesAsync();
        if (res <= 0) 
            return null;
        await _redisDb.JSON().SetAsync($"vendor:{vendor.Id}", "$", vendor);
        return vendor;
    }

    public async Task UpdateAsync(Vendor vendor)
    {
        await _context.Vendors.Where(v => v.Id == vendor.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(v => v.Name, vendor.Name)
            .SetProperty(v => v.OriginalName, vendor.OriginalName)
            .SetProperty(v => v.Description, vendor.Description)
            .SetProperty(v => v.CountryCode, vendor.CountryCode)
            .SetProperty(v => v.Logo, vendor.Logo)
        );
        
        var updatePayload = new 
        {
            Name = vendor.Name,
            OriginalName = vendor.OriginalName,
            Description = vendor.Description,
            CountryCode = vendor.CountryCode,
            Logo = vendor.Logo
        };
        
        await _redisDb.JSON().MergeAsync($"vendor:{vendor.Id}", "$", updatePayload);
    }

    public async Task DeleteAsync(int id)
    {
        // if (await _context.Vendors.Where(v => v.Id == id).ExecuteDeleteAsync() <=0)
        //     throw new InvalidOperationException("Object Not Found");
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            throw new InvalidOperationException("Object Not Found");
        _context.Remove(vendor);
        await  _context.SaveChangesAsync();
        await _redisDb.KeyDeleteAsync($"vendor:{id}");
    }
}