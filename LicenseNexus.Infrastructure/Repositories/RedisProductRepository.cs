using System.Text.Json;
using LicenseNexus.API.Helpers;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisProductRepository: IProductRepository
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly IDatabase _redisDb;
    private readonly IProductCacheService _cache;

    public RedisProductRepository(ExtendedSqlContext sqlContext, RedisContext redisContext, IProductCacheService cache)
    {
        _sqlContext = sqlContext;
        _redisDb = redisContext.Database;
        _cache = cache;
    }
    
    public async Task<ProductModel?> GetByIdAsync(int id)
    {
        var product = await _redisDb.JSON().GetAsync<ProductModel>($"product:{id}");
        if (product == null)
        {
            var dbProduct = await _sqlContext.Products.FirstOrDefaultAsync(p => p.Id == id);
    
            if (dbProduct != null)
            {
                await _cache.CacheProductByIdAsync(id);
                product = await _redisDb.JSON().GetAsync<ProductModel>($"product:{id}");
            }
        }
    
        return product;
    }

    public async Task<IEnumerable<ProductModel>> GetAllAsync()
    {
        var server = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First());
        var products = new List<ProductModel>();
        var keysToFetch = new List<string>();
    
        await foreach (var key in server.KeysAsync(pattern: "product:*"))
        {
            keysToFetch.Add(key!);
            
            if (keysToFetch.Count >= 1000)
            {
                await FetchAndAddProductsAsync(keysToFetch, products);
                keysToFetch.Clear();
            }
        }
        if (keysToFetch.Count > 0)
        {
            await FetchAndAddProductsAsync(keysToFetch, products);
        }

        return products;
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _sqlContext.Products.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsPriceAsync(long priceId, long productId, CancellationToken cancellationToken = default)
    {
        return await _sqlContext.ProductPrices.AnyAsync(p => p.Id == priceId && p.ProductId == productId, cancellationToken);
    }

    public async Task<PaginatedResult<ProductModel>> GetPaginatedAsync(
        int page, int pageSize, 
        int? categoryId, int? groupId, 
        int? vendorId, string? search,
        double? priceFrom, double? priceTo)
    {
        var skip = (page - 1) * pageSize;
        var take = pageSize;
        
        var keysToIntersect = new List<RedisKey>();
        
        keysToIntersect.Add("idx:price:products");

        if (categoryId.HasValue)
            keysToIntersect.Add($"idx:category:{categoryId.Value}:products");

        if (groupId.HasValue)
            keysToIntersect.Add($"idx:group:{groupId.Value}:products");

        if (vendorId.HasValue)
            keysToIntersect.Add($"idx:vendor:{vendorId.Value}:products");
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            var tokens = SearchTokenizer.Tokenize(search);
            foreach (var token in tokens)
            {
                keysToIntersect.Add($"idx:word:{token}:products");
            }
        }
        
        string searchKey;
        bool isTempKey = false;

        if (keysToIntersect.Count == 1)
        {
            searchKey = "idx:price:products";
        }
        else
        {
            searchKey = $"temp:search:{Guid.NewGuid()}";
            isTempKey = true;

            await _redisDb.SortedSetCombineAndStoreAsync(SetOperation.Intersect, searchKey, keysToIntersect.ToArray());
            await _redisDb.KeyExpireAsync(searchKey, TimeSpan.FromMinutes(1));
        }
        
        double minPrice = priceFrom.HasValue ? (double)priceFrom.Value : double.NegativeInfinity;
        double maxPrice = priceTo.HasValue ? (double)priceTo.Value : double.PositiveInfinity;
        
        var totalCount = await _redisDb.SortedSetLengthAsync(searchKey, minPrice, maxPrice);

        if (totalCount == 0)
        {
            if (isTempKey) await _redisDb.KeyDeleteAsync(searchKey);
            return new PaginatedResult<ProductModel> { Items = new(), TotalCount = 0, Page = page, PageSize = pageSize };
        }
        
        var productIds = await _redisDb.SortedSetRangeByScoreAsync(
            searchKey, 
            minPrice, 
            maxPrice, 
            Exclude.None, 
            StackExchange.Redis.Order.Ascending,
            skip, 
            take);
        
        var productKeys = productIds.Select(id => (RedisKey)$"product:{id}").ToArray();
        var jsonResults = await _redisDb.JSON().MGetAsync(productKeys, "$");
        var products = new List<ProductModel>();
        foreach (var result in jsonResults)
        {
            if (!result.IsNull)
            {
                var deserializedArray = JsonSerializer.Deserialize<ProductModel[]>((string)result!);
                if (deserializedArray != null && deserializedArray.Length > 0)
                {
                    products.Add(deserializedArray[0]);
                }
            }
        }
        
        if (isTempKey) await _redisDb.KeyDeleteAsync(searchKey);

        return new PaginatedResult<ProductModel>
        {
            Items = products!,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductModel?> AddAsync(ProductModel productModel)
    {
        var product = MapToDomain(productModel);
        _sqlContext.Products.Add(product);
        
        var res = await _sqlContext.SaveChangesAsync();
        if (res > 0)
        {
            productModel.Id = product.Id;
            await _cache.CacheProductModelAsync(productModel);
            return productModel;
        }

        return null;
    }

    public async Task UpdateAsync(ProductModel productModel)
    {
        var product = MapToDomain(productModel);
        _sqlContext.Products.Update(product);
        await _sqlContext.SaveChangesAsync();
        
        await _cache.RemoveProductCacheAsync(product.Id);
        await _cache.CacheProductByIdAsync(product.Id);
    }

    public async Task PatchAsync(int id,  ProductPatchFields updates)
    {
        var product = await _sqlContext.Products.FindAsync(id);
        if (product == null)
        {
            await _cache.RemoveProductCacheAsync(id);
            return;
        }
        
        var productKey = $"product:{id}";
        var isCached = await _redisDb.KeyExistsAsync(productKey);
        var pipeline = isCached ? new Pipeline(_redisDb) : null;
        var cacheTasks = new List<Task>();
        
        if (!string.IsNullOrWhiteSpace(updates.Sku))
        {
            product.Sku = updates.Sku;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Sku", $"\"{updates.Sku}\""));
        }

        if (!string.IsNullOrWhiteSpace(updates.Title))
        {
            if (isCached)
            {
                var oldTokens = SearchTokenizer.Tokenize(product.Title);
                foreach (var t in oldTokens)
                    cacheTasks.Add(pipeline!.Db.SetRemoveAsync($"idx:word:{t}:products", id));
            }

            product.Title = updates.Title;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Title", $"\"{updates.Title}\""));

            if (isCached)
            {
                var newTokens = SearchTokenizer.Tokenize(product.Title);
                foreach (var t in newTokens)
                    cacheTasks.Add(pipeline!.Db.SetAddAsync($"idx:word:{t}:products", id));
            }
        }
        
        if (!string.IsNullOrWhiteSpace(updates.ShortDescription))
        {
            product.ShortDescription = updates.ShortDescription;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.ShortDescription", $"\"{updates.ShortDescription}\"" ));
        }
        
        if (updates.QuantityMin.HasValue)
        {
            product.QuantityMin = (int)updates.QuantityMin;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.QuantityMin", (int)updates.QuantityMin));
        }
        
        if (updates.QuantityMax.HasValue)
        {
            product.QuantityMax = (int)updates.QuantityMax;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.QuantityMax", (int)updates.QuantityMax));
        }
        
        if (updates.StartDate.HasValue)
        {
            product.StartDate = updates.StartDate;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.StartDate", $"\"{updates.StartDate}\""));
        }
        
        if (updates.EndDate.HasValue)
        {
            product.EndDate = updates.EndDate;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.EndDate", $"\"{updates.EndDate}\""));
        }
        
        if (updates.IsPromo.HasValue)
        {
            product.IsPromo = (bool)updates.IsPromo;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.IsPromo", updates.IsPromo));
        }
        
        if (updates.IsTop.HasValue)
        {
            product.IsTop = (bool)updates.IsTop;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.IsTop", updates.IsTop));
        }
        
        if (updates.IsNew.HasValue)
        {
            product.IsNew = (bool)updates.IsNew;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.IsNew", updates.IsNew));
        }
        
        if (!string.IsNullOrWhiteSpace(updates.Logo))
        {
            product.Logo = updates.Logo;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.Logo", $"\"{updates.Logo}\""));
        }
        
        if (!string.IsNullOrWhiteSpace(updates.Author))
        {
            product.Author = updates.Author;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.Author", $"\"{updates.Author}\""));
        }
        
        if (updates.VendorId.HasValue)
        {
            var newVendor = await _sqlContext.Vendors
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == updates.VendorId);
            if (newVendor != null)
            {
                if (isCached) cacheTasks.Add(pipeline!.Db.SetRemoveAsync($"idx:vendor:{product.VendorId}:products", id));
                product.VendorId = newVendor.Id;
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.Vendor",  new VendorModel
                {
                    Id = newVendor.Id,
                    Name = newVendor.Name,
                    CountryCode = newVendor.CountryCode
                }));
                if (isCached) cacheTasks.Add(pipeline!.Db.SetAddAsync($"idx:vendor:{newVendor.Id}:products", id));
            }
        }
        
        if (updates.ProductTypeId.HasValue)
        {
            var newType = await _sqlContext.ProductTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == updates.ProductTypeId);
            if (newType != null)
            {
                if (isCached) cacheTasks.Add(pipeline!.Db.SetRemoveAsync($"idx:product_type:{product.ProductTypeId}:products", id));
                product.ProductTypeId = newType.Id;
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.TypeId", newType.Id));
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.TypeName", $"\"{newType.TypeName}\""));
                if (isCached) cacheTasks.Add(pipeline!.Db.SetAddAsync($"idx:product_type:{newType.Id}:products", id));
            }
        }
        
        if (updates.UnitMeasureId.HasValue)
        {
            var newUnitMeasure = await _sqlContext.UnitMeasures
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == updates.UnitMeasureId);
            if (newUnitMeasure != null)
            {
                product.UnitMeasureId = newUnitMeasure.Id;
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.UnitMeasureId", newUnitMeasure.Id));
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.UnitMeasureName", $"\"{newUnitMeasure.Name}\""));
            }
        }
        
        if (updates.CurrencyId.HasValue)
        {
            var newCurrency = await _sqlContext.Currencies
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == updates.CurrencyId);
            if (newCurrency != null)
            {
                product.CurrencyId = newCurrency.Id;
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Currency",  new CurrencyModel
                {
                    Id = newCurrency.Id,
                    Name = newCurrency.Name,
                    LiteralCode = newCurrency.LiteralCode
                }));
            }
        }
        
        if (updates.ProductGroupId.HasValue)
        {
            var newGroup = await _sqlContext.ProductGroups
                .AsNoTracking()
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == updates.ProductGroupId);
            
            if (newGroup != null && newGroup.Category != null)
            {
                if (isCached)
                {
                    var oldGroup = await _sqlContext.ProductGroups
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == product.ProductGroupId);
                    cacheTasks.Add(pipeline!.Db.SetRemoveAsync($"idx:group:{product.ProductGroupId}:products", id));
                    cacheTasks.Add(pipeline!.Db.SetRemoveAsync($"idx:category:{oldGroup?.CategoryId}:products", id));
                }

                product.ProductGroupId = newGroup.Id;
                
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.Group",  new GroupModel()
                {
                    Id = newGroup.Id,
                    Name = newGroup.Name,
                    CategoryId = newGroup.CategoryId,
                    CategoryName = newGroup.Category.CategoryName
                }));

                if (isCached)
                {
                    cacheTasks.Add(pipeline!.Db.SetAddAsync($"idx:group:{newGroup.Id}:products", id));
                    cacheTasks.Add(pipeline!.Db.SetAddAsync($"idx:category:{newGroup.CategoryId}:products", id));
                }
            }
        }

        await _sqlContext.SaveChangesAsync();
        if (isCached)
        {
            pipeline!.Execute();
            await Task.WhenAll(cacheTasks);
        }
        else
        {
            await _cache.CacheProductByIdAsync(id);
        }
        
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _sqlContext.Products.FindAsync(id);
        if (product != null)
        {
            _sqlContext.Products.Remove(product);
            await _sqlContext.SaveChangesAsync();
            
            await _cache.RemoveProductCacheAsync(product.Id);
        }
    }

    public async Task<ProductPrice?> GetPriceAsync(int productId, int priceId)
    {
        return await _sqlContext.ProductPrices.FirstOrDefaultAsync(p => p.Id == priceId && p.ProductId == productId);
    }

    public async Task<ProductPrice?> AddPrice(ProductPrice price)
    {
        _sqlContext.ProductPrices.Add(price);
        var res = await _sqlContext.SaveChangesAsync();
        if (res > 0)
        {
            var productKey = $"product:{price.ProductId}";
            await _redisDb.JSON().ArrAppendAsync(productKey, "$.Prices", new ProductPriceModel
            {
                Id = price.Id,
                Price = price.Price,
                TermDuration = price.TermDuration,
                BillingPlan = price.BillingPlan,
                CountryCode = price.CountryCode,
                Segment = price.Segment,
                StartDate = price.StartDate
            });
            return price;
        }
            
        return null;
    }

    public async Task UpdatePrice(ProductPrice price)
    {
        var res = await _sqlContext.ProductPrices.Where(p => p.Id == price.Id && p.ProductId == price.ProductId).ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.Price, price.Price)
            .SetProperty(p => p.TermDuration, price.TermDuration)
            .SetProperty(p => p.BillingPlan, price.BillingPlan)
            .SetProperty(p => p.CountryCode, price.CountryCode)
            .SetProperty(p => p.Segment, price.Segment)
            .SetProperty(p => p.StartDate, price.StartDate)
        );

        if (res > 0)
        {
            await _redisDb.JSON().SetAsync($"product:{price.ProductId}", $"$.Prices[?(@.Id=={price.Id})]", price);
        }
    }

    public async Task DeletePrice(int productId, int priceId)
    {
        var res = await _sqlContext.ProductPrices.Where(p => p.Id == priceId).ExecuteDeleteAsync();
        if (res > 0)
            await _redisDb.JSON().DelAsync($"product:{productId}", $"$.Prices[?(@.Id=={priceId})]");
    }

    public async Task AddTag(int productId, int tagId)
    {
        var tag = await _sqlContext.Tags.FindAsync(tagId);
        if (tag == null) throw new Exception("Tag not found");
        
        await _sqlContext.ProductTags.AddAsync(new ProductTag { TagId = tagId, ProductId = productId });
        var res = await _sqlContext.SaveChangesAsync();
        
        if (res > 0)
        {
            var productKey = $"product:{productId}";
            await _redisDb.JSON().ArrAppendAsync(productKey, "$.Tags", new TagModel
            {
                Id = tagId,
                Name = tag.Name
            });
        }
    }

    public async Task DeleteTag(int productId, int tagId)
    {
        var res = await _sqlContext.ProductTags.Where(t => t.ProductId == productId && t.TagId == tagId).ExecuteDeleteAsync();
        if (res > 0)
            await _redisDb.JSON().DelAsync($"product:{productId}", $"$.Tags[?(@.Id=={tagId})]");
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
            ProductTypeId = model.Classification.TypeId,
            UnitMeasureId = model.Classification.UnitMeasureId,
            CurrencyId = model.Currency.Id,
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
    
    private async Task FetchAndAddProductsAsync(List<string> keys, List<ProductModel> products)
    {
        var jsonResults = await _redisDb.JSON().MGetAsync(keys.Select(k => (RedisKey)k).ToArray(), "$");

        foreach (var result in jsonResults)
        {
            if (result.IsNull) continue;
            var deserializedArray = JsonSerializer.Deserialize<ProductModel[]>((string)result!);
            if (deserializedArray != null && deserializedArray.Length > 0)
            {
                products.Add(deserializedArray[0]);
            }
        }
    }
}