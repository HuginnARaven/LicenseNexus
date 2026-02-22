using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using NRedisStack;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Services;

public class RedisProductSyncService : IProductSyncService
{
    private readonly IDatabase _redisDb;
    private readonly ExtendedSqlContext _sqlContext;
    
    public RedisProductSyncService(ExtendedSqlContext sqlContext, RedisContext redisContext)
    {
        _sqlContext = sqlContext;
        _redisDb = redisContext.Database;
    }
    
    public async Task UpdateVendorAsync(Vendor vendor, CancellationToken ct)
    {
        var indexKey = $"idx:vendor:{vendor.Id}:products";
        var productIds = await _redisDb.SetMembersAsync(indexKey);
        if (productIds.Length == 0) return;
        
        var pipeline = new Pipeline(_redisDb);
        var updateTasks = new List<Task>();
        var newVendor = new VendorModel
        {
            Id = vendor.Id,
            Name = vendor.Name,
            CountryCode = vendor.CountryCode,
        };

        foreach (var productId in productIds)
        {
            var productKey = $"product:{productId}";

            updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Classification.Vendor", newVendor));
        }
        
        pipeline.Execute();
        await Task.WhenAll(updateTasks);
    }

    public async Task UpdateCategoryAsync(Category category, CancellationToken ct)
    {
        var indexKey = $"idx:category:{category.Id}:products";
        var productIds = await _redisDb.SetMembersAsync(indexKey);
        if (productIds.Length == 0) return;
        
        
        var pipeline = new Pipeline(_redisDb);
        var updateTasks = new List<Task>();
        
        foreach (var productId in productIds)
        {
            var productKey = $"product:{productId}";
            updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Classification.Group.CategoryName", $"\"{category.CategoryName}\""));
        }

        pipeline.Execute();
        await Task.WhenAll(updateTasks);
    }

    public async Task UpdateGroupAsync(ProductGroup group, CancellationToken ct)
    {
        var indexKey = $"idx:group:{group.Id}:products";
        var productIds = await _redisDb.SetMembersAsync(indexKey);
    
        if (productIds.Length == 0) return;
    
        var pipeline = new Pipeline(_redisDb);
        var updateTasks = new List<Task>();
    
        foreach (var productId in productIds)
        {
            Console.WriteLine("productId" + productId);
            var productKey = $"product:{productId}";
            updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Classification.Group.Name", $"\"{group.Name}\""));
        }
    
        pipeline.Execute();
        await Task.WhenAll(updateTasks);
    }

    public async Task UpdateProductTypeAsync(ProductType productType, CancellationToken ct)
    {
        var indexKey = $"idx:product_type:{productType.Id}:products";
        var productIds = await _redisDb.SetMembersAsync(indexKey);
        if (productIds.Length == 0) return;
        
        var pipeline = new Pipeline(_redisDb);
        var updateTasks = new List<Task>();
        
        foreach (var productId in productIds)
        {
            var productKey = $"product:{productId}";
            updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Classification.TypeName", $"\"{productType.TypeName}\""));
        }
        
        pipeline.Execute();
        await Task.WhenAll(updateTasks);
    }

    public async Task UpdateUnitMeasureAsync(UnitMeasure unitMeasure, CancellationToken ct)
    {

        var productIds = await _sqlContext.Products
            .AsNoTracking()
            .Where(p => p.UnitMeasureId == unitMeasure.Id)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken: ct);

        var pipeline = new Pipeline(_redisDb);
        var updateTasks = new List<Task>();

        foreach (var productId in productIds)
        {
            var productKey = $"product:{productId}";
            updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Classification.UnitMeasureName", $"\"{unitMeasure.Name}\""));
        }
        
        pipeline.Execute();
        await Task.WhenAll(updateTasks);
    }

    public async Task UpdateCurrencyAsync(Currency currency, CancellationToken ct)
    {
        var productIds = await _sqlContext.Products
            .AsNoTracking()
            .Where(p => p.CurrencyId == currency.Id)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken: ct);
        
        var pipeline = new Pipeline(_redisDb);
        var updateTasks = new List<Task>();
        var newCurrency = new CurrencyModel
        {
            Id = currency.Id,
            LiteralCode = currency.LiteralCode,
            Name = currency.Name
        };

        foreach (var productId in productIds)
        {
            var productKey = $"product:{productId}";
            updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Currency", newCurrency));
        }
        
        pipeline.Execute();
        await Task.WhenAll(updateTasks);
    }
}