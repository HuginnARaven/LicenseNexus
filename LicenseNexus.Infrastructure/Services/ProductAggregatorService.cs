using System.Text.Json;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.RedisEntities;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Services;

public class ProductAggregatorService : IProductAggregatorService
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly IDatabase _redisDb;

    public ProductAggregatorService(ExtendedSqlContext sqlContext, RedisContext redisContext)
    {
        _sqlContext = sqlContext;
        _redisDb = redisContext.Database;
    }

    public async Task AggregateProductAsync(int productId)
    {
        var productEntity = await GetBaseQuery()
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (productEntity == null) return;
            
        var redisModel = MapToRedisModel(productEntity);
        var json = JsonSerializer.Serialize(redisModel);

        await _redisDb.StringSetAsync($"product:{productEntity.Id}", json);
        
        if (productEntity.ProductGroup != null)
        {
            var indexKey = $"category:{productEntity.ProductGroup.CategoryId}:products";
            await _redisDb.SetAddAsync(indexKey, productEntity.Id.ToString());
        }
    }

    public async Task AggregateAllProductsAsync()
    {
        var allProducts = await GetBaseQuery().ToListAsync();

        foreach (var product in allProducts)
        {
            var redisModel = MapToRedisModel(product);
            var json = JsonSerializer.Serialize(redisModel);
            await _redisDb.StringSetAsync($"product:{product.Id}", json);
        }
    }
    
    public async Task BuildIndexesAsync()
    {
        var products = await _sqlContext.Products
            .Include(p => p.ProductGroup)
            .Select(p => new 
            { 
                p.Id, 
                CategoryId = p.ProductGroup.CategoryId 
            })
            .ToListAsync();
        
        var categoryGroups = products.GroupBy(p => p.CategoryId);

        foreach (var group in categoryGroups)
        {
            var categoryId = group.Key;
            var redisKey = $"category:{categoryId}:products";
            
            var productIds = group.Select(x => (RedisValue)x.Id.ToString()).ToArray();
            await _redisDb.KeyDeleteAsync(redisKey);
            
            if (productIds.Any())
            {
                await _redisDb.SetAddAsync(redisKey, productIds);
            }
        }
    }

    private IQueryable<Domain.Entities.Product> GetBaseQuery()
    {
        return _sqlContext.Products
            .Include(p => p.Vendor)
            .Include(p => p.ProductType)
            .Include(p => p.UnitMeasure)
            .Include(p => p.Currency)
            .Include(p => p.ProductGroup).ThenInclude(pg => pg.Category)
            .Include(p => p.Prices)
            .Include(p => p.FullDescriptions)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .AsSplitQuery();
    }

    private ProductModel MapToRedisModel(Domain.Entities.Product p)
    {
        return new ProductModel
        {
            Id = p.Id,
            Sku = p.Sku ?? "",
            Title = p.Title,
            IsActive = (p.ProductGroup?.IsActive ?? false) && (p.ProductGroup?.Category?.IsActive ?? false),
                
            Tags = p.ProductTags.Select(pt => pt.Tag?.Name ?? "").ToList(),
                
            Classification = new ClassificationModel
            {
                TypeName = p.ProductType?.TypeName ?? "",
                UnitMeasureName = p.UnitMeasure?.Name ?? "",
                Vendor = new VendorModel
                {
                    Id = p.VendorId,
                    Name = p.Vendor?.Name ?? "",
                    CountryCode = p.Vendor?.CountryCode ?? ""
                },
                Group = new GroupModel
                {
                    Id = p.ProductGroupId,
                    Name = p.ProductGroup?.Name ?? "",
                    CategoryName = p.ProductGroup?.Category?.CategoryName ?? ""
                }
            },
                
            Attributes = new AttributesModel
            {
                QuantityMin = p.QuantityMin,
                QuantityMax = p.QuantityMax,
                IsPromo = p.IsPromo,
                IsTop = p.IsTop,
                IsNew = p.IsNew,
                Logo = p.Logo,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedDate = p.CreatedDate,
                Author = p.Author
            },
                
            Descriptions = p.FullDescriptions.Select(d => new DescriptionModel
            {
                Description = p.ShortDescription ?? "", // Короткий опис з головної таблиці
                FullText = d.FullText,
                LanguageCode = d.LanguageCode
            }).ToList(),
                
            Prices = p.Prices.Select(pr => new ProductPriceModel
            {
                Price = pr.Price,
                CurrencyCode = p.Currency?.LiteralCode ?? "",
                TermDuration = pr.TermDuration,
                BillingPlan = pr.BillingPlan,
                Segment = pr.Segment,
                CountryCode = pr.CountryCode,
                StartDate = pr.StartDate
            }).ToList()
        };
    }
}