using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;
using LicenseNexus.Infrastructure.Data.Contexts;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisCartRepository: ICartRepository
{
    private readonly IDatabase _redisDb;

    public RedisCartRepository(RedisContext redisContext)
    {
        _redisDb = redisContext.Database;
    }
    
    public async Task<IEnumerable<CartItem>> GetCartAsync(int customerId)
    {
        var key = $"cart:{customerId}";
        var hashEntries = await _redisDb.HashGetAllAsync(key);
        
        return hashEntries.Select(entry => new CartItem
        {
            ProductId = int.Parse(entry.Name.ToString()), 
            Quantity = int.Parse(entry.Value.ToString()) 
        });
    }

    public async Task AddToCartAsync(int customerId, int productId, int quantity)
    {
        var key = $"cart:{customerId}";
        await _redisDb.HashIncrementAsync(key, productId, quantity);
    }

    public async Task RemoveFromCartAsync(int customerId, int productId)
    {
        var key = $"cart:{customerId}";
        await _redisDb.HashDeleteAsync(key, productId);
    }

    public async Task ClearCartAsync(int customerId)
    {
        var key = $"cart:{customerId}";
        await _redisDb.KeyDeleteAsync(key);
    }
}