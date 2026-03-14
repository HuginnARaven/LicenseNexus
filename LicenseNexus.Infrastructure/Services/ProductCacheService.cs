using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Models;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;

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
        var productKey = $"product:{productModel.Id}";
        await _redisDb.JSON().SetAsync(productKey, "$", productModel);
    }
    
    public async Task RemoveProductCacheAsync(int productId)
    {
        var productKey = $"product:{productId}";
        if (!await _redisDb.KeyExistsAsync(productKey)) return;
        await _redisDb.KeyDeleteAsync(productKey);
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

        await CreateProductIndexAsync();
    }
    
    public async Task CreateProductIndexAsync()
    {
        var ft = _redisDb.FT();
    
        try
        {
            await ft.InfoAsync("idx:products");
        }
        catch
        {
            var schema = new Schema()
                .AddNumericField(new FieldName("$.Classification.Group.CategoryId", "CategoryId"))
                .AddNumericField(new FieldName("$.Classification.Group.Id", "GroupId"))
                .AddNumericField(new FieldName("$.Classification.Vendor.Id", "VendorId"))
                .AddNumericField(new FieldName("$.Classification.TypeId", "TypeId"))

                .AddNumericField(new FieldName("$.Prices[*].Price", "Price"))

                .AddTextField(new FieldName("$.Title", "Title"));

            var ftParams = new FTCreateParams()
                .On(IndexDataType.JSON)
                .Prefix("product:");

            await ft.CreateAsync("idx:products", ftParams, schema);
        }
    }

    private IQueryable<Product> GetBaseQuery()
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

    private ProductModel MapToRedisModel(Product p)
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
                    CategoryId = p.ProductGroup?.CategoryId ?? 0,
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