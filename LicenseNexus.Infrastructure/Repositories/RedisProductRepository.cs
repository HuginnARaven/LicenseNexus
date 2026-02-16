using System.Text.Json;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisProductRepository: IProductRepository
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly IDatabase _redisDb;
    private readonly IProductAggregatorService _aggregator;

    public RedisProductRepository(ExtendedSqlContext sqlContext, RedisContext redisContext, IProductAggregatorService aggregator)
    {
        _sqlContext = sqlContext;
        _redisDb = redisContext.Database;
        _aggregator = aggregator;
    }
    
    public async Task<ProductModel?> GetByIdAsync(int id)
    {
        var json = await _redisDb.StringGetAsync($"product:{id}");

        if (json.IsNullOrEmpty)
        {
            var product = await _sqlContext.Products
                .Include(p => p.Vendor) 
                .FirstOrDefaultAsync(p => p.Id == id);
        
            if (product != null)
            {
                await _aggregator.AggregateProductAsync(id);
                json = await _redisDb.StringGetAsync($"product:{id}");
            }
        }
        
        if (json.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<ProductModel>((string)json!);
    }

    public async Task<IEnumerable<ProductModel>> GetAllAsync()
    {
        var server = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: "product:*");
        
        var products = new List<ProductModel>();

        foreach (var key in keys)
        {
            var json = await _redisDb.StringGetAsync(key);
            if (!json.IsNullOrEmpty)
            {
                var product = JsonSerializer.Deserialize<ProductModel>((string)json!);
                if (product != null)
                {
                    products.Add(product);
                }
            }
        }

        return products;
    }

    public async Task AddAsync(ProductModel productModel)
    {
        var product = MapToDomain(productModel);
        _sqlContext.Products.Add(product);
        await _sqlContext.SaveChangesAsync();
        
        await _aggregator.AggregateProductAsync(product.Id);
    }

    public async Task UpdateAsync(ProductModel productModel)
    {
        var product = MapToDomain(productModel);
        _sqlContext.Products.Update(product);
        await _sqlContext.SaveChangesAsync();
        
        await _aggregator.AggregateProductAsync(product.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _sqlContext.Products.FindAsync(id);
        if (product != null)
        {
            _sqlContext.Products.Remove(product);
            await _sqlContext.SaveChangesAsync();
            
            await _redisDb.KeyDeleteAsync($"product:{id}");
        }
    }

    private Product MapToDomain(ProductModel model)
    {
        return new Product
        {
            Id = model.Id,
            Sku = model.Sku,
            Title = model.Title,
            ShortDescription = model.Attributes.ShortDescription,
            VendorId = model.Classification.Vendor.Id,
            ProductTypeId = model.Classification.TypeId ?? 0,
            UnitMeasureId = model.Classification.UnitMeasureId ?? 0,
            CurrencyId = model.CurrencyId ?? 0,
            ProductGroupId = model.Classification.Group.Id,
            QuantityMin = model.Attributes.QuantityMin,
            QuantityMax = model.Attributes.QuantityMax,
            StartDate = model.Attributes.StartDate,
            EndDate = model.Attributes.EndDate,
            IsPromo = model.Attributes.IsPromo,
            IsTop = model.Attributes.IsTop,
            IsNew = model.Attributes.IsNew,
            Logo = model.Attributes.Logo,
            CreatedDate = model.Attributes.CreatedDate,
            Author = model.Attributes.Author,
        };
    }
}