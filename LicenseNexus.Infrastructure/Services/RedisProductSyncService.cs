using System.Text.Json;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;
using LicenseNexus.Infrastructure.Data.Contexts;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Services;

public class RedisProductSyncService : IProductSyncService
{
    private readonly IDatabase _redisDb;
    public RedisProductSyncService(RedisContext redisContext)
    {

        _redisDb = redisContext.Database;
    }
    
    public async Task UpdateVendorAsync(Vendor vendor, CancellationToken ct) //TODO: use after migrated to RedisJSON
    {
        var db = _redisDb;
        var indexKey = $"idx:vendor:{vendor.Id}:product";
        
        var productIds = await db.SetMembersAsync(indexKey);
        if (productIds.Length == 0) return;
        
        var batch = db.CreateBatch();
        var updateTasks = new List<Task>();

        foreach (var productId in productIds)
        {
            var productKey = $"product:{productId}";
            var oldJson = await db.StringGetAsync(productKey);
            
            //updateTasks.Add(batch.JsonSetAsync(productKey, "$.Vendor.Name", $"\"{newVendorName}\""));
        }

        // Відправляємо батч в Redis
        batch.Execute();
        
        // Чекаємо завершення всіх операцій оновлення
        await Task.WhenAll(updateTasks);
    }
}