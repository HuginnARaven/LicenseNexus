using System.Text.Json;
using LicenseNexus.API.Helpers;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Models;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Services;

public class ProductCacheService : IProductCacheService
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly IDatabase _redisDb;

    public ProductCacheService(ExtendedSqlContext sqlContext, RedisContext redisContext)
    {
        _sqlContext = sqlContext;
        _redisDb = redisContext.Database;
    }

    public async Task<ProductModel?> CacheProductByIdAsync(int productId)
    {
        var productEntity = await GetBaseQuery()
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (productEntity == null) return null;
            
        var redisModel = MapToRedisModel(productEntity);
        
        await CacheProductModelAsync(redisModel);
        return redisModel;
    }

    public async Task CacheProductModelAsync(ProductModel productModel)
    {
        var cacheTasks = new List<Task>();
        var pipeline = new Pipeline(_redisDb);
        
        var productKey = $"product:{productModel.Id}";
        cacheTasks.Add(pipeline.Json.SetAsync(productKey, "$", productModel));
        
        if (productModel.Classification?.Group.CategoryId != null)
        {
            var categoryIndexKey = $"idx:category:{productModel.Classification.Group.CategoryId}:products";
            cacheTasks.Add(pipeline.Db.SetAddAsync(categoryIndexKey, productModel.Id));
        }
        
        if (productModel.Classification?.Group.Id != null)
        {
            var groupKey = $"idx:group:{productModel.Classification?.Group.Id}:products";
            cacheTasks.Add(pipeline.Db.SetAddAsync(groupKey, productModel.Id));
        }
        
        if (productModel.Classification?.TypeId != null)
        {
            var typeKey = $"idx:product_type:{productModel.Classification?.TypeId}:products";
            cacheTasks.Add(pipeline.Db.SetAddAsync(typeKey, productModel.Id));
        }
        
        if (productModel.Classification?.Vendor?.Id != null)
        {
            var vendorKey = $"idx:vendor:{productModel.Classification?.Vendor.Id}:products";
            cacheTasks.Add(pipeline.Db.SetAddAsync(vendorKey, productModel.Id));
        }
        
        var minPrice = productModel.Prices?
            .Where(p => p.Price > 0)
            .Select(p => p.Price)
            .DefaultIfEmpty(0)
            .Min();
        
        if (minPrice > 0)
        {
            cacheTasks.Add(pipeline.Db.SortedSetAddAsync("idx:price:products", productModel.Id, (double)minPrice));
        }
        
        var tokens = SearchTokenizer.Tokenize(productModel.Title);
        foreach (var token in tokens)
        {
            cacheTasks.Add(pipeline.Db.SetAddAsync($"idx:word:{token}:products", productModel.Id));
        }
        
        pipeline.Execute();
        await Task.WhenAll(cacheTasks);
    }
    
    public async Task RemoveProductCacheAsync(int productId)
    {
        var productKey = $"product:{productId}";
        if (!await _redisDb.KeyExistsAsync(productKey)) return;
        var oldModel = await _redisDb.JSON().GetAsync<ProductModel?>(productKey);
        if (oldModel == null) return;
        
        var batch = _redisDb.CreateBatch();
        
        if (oldModel.Classification.Group?.CategoryId != null)
        {
            var oldCatKey = $"idx:category:{oldModel.Classification.Group?.CategoryId}:products";
            _ = batch.SetRemoveAsync(oldCatKey, productId);
        }
        
        if (oldModel.Classification.Group?.Id != null)
        {
            var oldGroupKey = $"idx:group:{oldModel.Classification.Group?.Id}:products";
            _ = batch.SetRemoveAsync(oldGroupKey, productId);
        }
        
        if (oldModel.Classification?.TypeId != null)
        {
            var oldTypeKey = $"idx:product_type:{oldModel.Classification?.TypeId}:products";
            _ = batch.SetRemoveAsync(oldTypeKey, productId);
        }
        
        if (oldModel.Classification?.Vendor?.Id != null)
        {
            var oldVendorKey = $"idx:vendor:{oldModel.Classification?.Vendor.Id}:products";
            _ = batch.SetRemoveAsync(oldVendorKey, productId);
        }
        
        var oldTokens = SearchTokenizer.Tokenize(oldModel.Title);
        foreach (var token in oldTokens)
        {
            _ = batch.SetRemoveAsync($"idx:word:{token}:products", productId);
        }
        
        _ = batch.SortedSetRemoveAsync("idx:price:products", productId);
        
        _ = batch.KeyDeleteAsync(productKey);
        
        batch.Execute();
        await Task.CompletedTask;
    }

    public async Task CacheAllProductsAsync()
    {
        var allProducts = await GetBaseQuery().ToListAsync();
        var allVendors = await _sqlContext.Vendors.ToListAsync();
        var allProductTypes = await _sqlContext.ProductTypes.ToListAsync();
        var allCategories = await _sqlContext.Categories.Include(c => c.ProductGroups).ToListAsync();
        var allUnitMeasures = await _sqlContext.UnitMeasures.ToListAsync();
        var allCurrencies = await _sqlContext.Currencies.ToListAsync();

        var pipeline = new Pipeline(_redisDb);
        List<Task> cacheTasks = new List<Task>();


        foreach (var vendor in allVendors)
        {
            cacheTasks.Add(pipeline.Json.SetAsync($"vendor:{vendor.Id}", "$", vendor));
        }
        
        foreach (var productType in allProductTypes)
        {
            cacheTasks.Add(pipeline.Json.SetAsync($"product_type:{productType.Id}", "$", productType));
        }

        foreach (var category in allCategories)
        {
            var writeCategory = new Category()
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                IsActive = category.IsActive,
                Description = category.Description,
                CreatedDate = category.CreatedDate,
                Author = category.Author,
                ProductGroups = category.ProductGroups.Select(pg => new ProductGroup
                {
                    Id = pg.Id,
                    Name = pg.Name,
                    IsActive = pg.IsActive,
                    Note = pg.Note,
                    CreatedDate = pg.CreatedDate,
                    Author = pg.Author,
                    CategoryId = pg.CategoryId
                }).ToList()
            };
            
            var hashEntries = category.ProductGroups
                .Select(g => new HashEntry(g.Id.ToString(), category.Id.ToString()))
                .ToArray();
            
            cacheTasks.Add(pipeline.Json.SetAsync($"category:{category.Id}", "$", writeCategory));
            
            if (hashEntries.Any())
            {
                cacheTasks.Add(pipeline.Db.HashSetAsync("pg_to_category_map", hashEntries));
            }
        }
        
        foreach (var unitMeasure in allUnitMeasures)
        {
            cacheTasks.Add(pipeline.Json.SetAsync($"unit_measure:{unitMeasure.Id}", "$", unitMeasure));
        }
        
        foreach (var currency in allCurrencies)
        {
            cacheTasks.Add(pipeline.Json.SetAsync($"currency:{currency.Id}", "$", currency));
        }
        
        pipeline.Execute();
        await Task.WhenAll(cacheTasks);
        
        foreach (var product in allProducts)
        {
            var redisModel = MapToRedisModel(product);
            await RemoveProductCacheAsync(product.Id);
            await CacheProductModelAsync(redisModel);
        }
    }
    
    public async Task BuildIndexesAsync()  // obsolete
    {
        var products = await _sqlContext.Products
            .Include(p => p.ProductGroup)
            .Select(p => new 
            { 
                p.Id, 
                CategoryId = p.ProductGroup!.CategoryId 
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
    
    private static IEnumerable<string> OLDTokenize(string text) // obsolete
    {
        if (string.IsNullOrWhiteSpace(text)) return Enumerable.Empty<string>();

        return text.ToLower()
            .Split(new[] { ' ', ',', '.', '-', '_', '/', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .Distinct();
    }

    private IQueryable<Domain.Entities.Product> GetBaseQuery()
    {
        return _sqlContext.Products
            .Include(p => p.Vendor)
            .Include(p => p.ProductType)
            .Include(p => p.UnitMeasure)
            .Include(p => p.Currency)
            .Include(p => p.ProductGroup).ThenInclude(pg => pg!.Category)
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
            
            Classification = new ClassificationModel
            {
                TypeId = p.ProductTypeId,
                TypeName = p.ProductType?.TypeName ?? "",
                UnitMeasureId = p.UnitMeasureId,
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
                    CategoryId = p.ProductGroupId,
                    CategoryName = p.ProductGroup?.Category?.CategoryName ?? ""
                }
            },
            
            Attributes = new AttributesModel
            {
                ShortDescription = p.ShortDescription ?? "",
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
            
            Tags = p.ProductTags.Select(pt => new TagModel
            {
                Id = pt.TagId,
                Name = pt.Tag?.Name ?? ""
            }).ToList(),
            
            Descriptions = p.FullDescriptions.Select(d => new DescriptionModel
            {
                Id = d.Id,
                FullText = d.FullText,
                LanguageCode = d.LanguageCode
            }).ToList(),
            
            Currency = new CurrencyModel
            {
                Id = p.CurrencyId,
                LiteralCode = p.Currency?.LiteralCode ?? "",
                Name = p.Currency?.Name ?? ""
            },
            
            Prices = p.Prices.Select(pr => new ProductPriceModel
            {
                Id = pr.Id,
                Price = pr.Price,
                TermDuration = pr.TermDuration,
                BillingPlan = pr.BillingPlan,
                Segment = pr.Segment,
                CountryCode = pr.CountryCode,
                StartDate = pr.StartDate
            }).ToList()
        };
    }
}