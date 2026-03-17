using System.Text.Json;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
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
        int batchSize = 2500;
        int skip = 0;
        long totalResults = 0;

        var newVendor = JsonSerializer.Serialize(new VendorModel
        {
            Id = vendor.Id,
            Name = vendor.Name,
            CountryCode = vendor.CountryCode,
        });
        
        do
        {
            ct.ThrowIfCancellationRequested();
            
            var query = new Query($"@VendorId:{{{vendor.Id}}}")
                .Limit(skip, batchSize)
                .ReturnFields("Id");
            
            var searchResult = await _redisDb.FT().SearchAsync("idx:products", query);
            totalResults = searchResult.TotalResults;
            if (searchResult.Documents.Count == 0) break;
            
            //var pipeline = new Pipeline(_redisDb);
            var updateTasks = new List<Task>();
        
            
            foreach (var doc in searchResult.Documents)
            {
                var productKey = doc.Id; 
                //updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Classification.Vendor", newVendor));
                var task = _redisDb.JSON().SetAsync(productKey, "$.Classification.Vendor", newVendor);
                updateTasks.Add(task);
            }
            
            //pipeline.Execute();
            await Task.WhenAll(updateTasks);
        
            skip += batchSize;
        
        } while (skip < totalResults);
    }
    
    public async Task UpdateCategoryAsync(Category category, CancellationToken ct)
    {
        int batchSize = 10000;
        int skip = 0;
        long totalResults = 0;

        var categoryName = JsonSerializer.Serialize(category.CategoryName);

        do
        {
            var query = new Query($"@CategoryId:{{{category.Id}}}")
                .Limit(skip, batchSize)
                .ReturnFields("Id");
            var searchResult = await _redisDb.FT().SearchAsync("idx:products", query);
            totalResults = searchResult.TotalResults;
            if (searchResult.Documents.Count == 0) break;
            
            var pipeline = new Pipeline(_redisDb);
            var updateTasks = new List<Task>();

            foreach (var doc in searchResult.Documents)
            {
                var productKey = doc.Id; 
                updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Classification.Group.CategoryName", categoryName));
            }

            pipeline.Execute();
            await Task.WhenAll(updateTasks);

            skip += batchSize;

        } while (skip < totalResults);
    }
    
    public async Task UpdateGroupAsync(ProductGroup group, CancellationToken ct)
    {
        int batchSize = 10000;
        int skip = 0;
        long totalResults = 0;

        var groupName = JsonSerializer.Serialize(group.Name);

        do
        {
            var query = new Query($"@GroupId:{{{group.Id}}}")
                .Limit(skip, batchSize)
                .ReturnFields("Id");
            var searchResult = await _redisDb.FT().SearchAsync("idx:products", query);
            totalResults = searchResult.TotalResults;
            if (searchResult.Documents.Count == 0) break;
            
            var pipeline = new Pipeline(_redisDb);
            var updateTasks = new List<Task>();

            foreach (var doc in searchResult.Documents)
            {
                var productKey = doc.Id; 
                updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Classification.Group.Name", groupName));
            }

            pipeline.Execute();
            await Task.WhenAll(updateTasks);

            skip += batchSize;

        } while (skip < totalResults);
    }
    
    public async Task UpdateProductTypeAsync(ProductType productType, CancellationToken ct)
    {
        int batchSize = 10000;
        int skip = 0;
        long totalResults = 0;

        var productTypeName = JsonSerializer.Serialize(productType.TypeName);

        do
        {
            var query = new Query($"@TypeId:{{{productType.Id}}}")
                .Limit(skip, batchSize)
                .ReturnFields("Id");
            var searchResult = await _redisDb.FT().SearchAsync("idx:products", query);
            totalResults = searchResult.TotalResults;
            if (searchResult.Documents.Count == 0) break;
            
            var pipeline = new Pipeline(_redisDb);
            var updateTasks = new List<Task>();

            foreach (var doc in searchResult.Documents)
            {
                var productKey = doc.Id; 
                updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Classification.TypeName", productTypeName));
            }

            pipeline.Execute();
            await Task.WhenAll(updateTasks);

            skip += batchSize;

        } while (skip < totalResults);
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
        var unitMeasureName = JsonSerializer.Serialize(unitMeasure.Name);
        
        foreach (var productId in productIds)
        {
            var productKey = $"product:{productId}";
            updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Classification.UnitMeasureName", unitMeasureName));
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
        var newCurrency = JsonSerializer.Serialize(new CurrencyModel
        {
            Id = currency.Id,
            LiteralCode = currency.LiteralCode,
            Name = currency.Name
        });

        foreach (var productId in productIds)
        {
            var productKey = $"product:{productId}";
            updateTasks.Add(pipeline.Json.SetAsync(productKey, "$.Currency", newCurrency));
        }
        
        pipeline.Execute();
        await Task.WhenAll(updateTasks);
    }

    public async Task UpdateTagAsync(Tag tag, CancellationToken ct)
    {
        var productIds = await _sqlContext.ProductTags
            .AsNoTracking()
            .Where(p => p.TagId == tag.Id)
            .Select(p => p.ProductId)
            .ToListAsync(cancellationToken: ct);
        var pipeline = new Pipeline(_redisDb);
        var updateTasks = new List<Task>();
        var tagName = JsonSerializer.Serialize(tag.Name);

        foreach (var productId in productIds)
        {
            var productKey = $"product:{productId}";
            updateTasks.Add(pipeline.Json.SetAsync(productKey, $"$.Tags[?(@.Id=={tag.Id})].Name", tagName));
        }
        
        pipeline.Execute();
        await Task.WhenAll(updateTasks);
    }
}