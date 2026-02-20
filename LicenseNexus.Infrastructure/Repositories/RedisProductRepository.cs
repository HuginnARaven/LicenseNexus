using System.Text.Json;
using LicenseNexus.API.Helpers;
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
    private readonly IProductCacheService _cache;

    public RedisProductRepository(ExtendedSqlContext sqlContext, RedisContext redisContext, IProductCacheService cache)
    {
        _sqlContext = sqlContext;
        _redisDb = redisContext.Database;
        _cache = cache;
    }
    
    public async Task<ProductModel?> GetByIdAsync(int id)
    {
        var json = await _redisDb.StringGetAsync($"product:{id}");

        if (json.IsNullOrEmpty)
        {
            var product = await _sqlContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        
            if (product != null)
            {
                await _cache.CacheProductByIdAsync(id);
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
        Console.WriteLine(searchKey);
        Console.WriteLine(totalCount);
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
        var jsonResults = await _redisDb.StringGetAsync(productKeys);

        var products = jsonResults
            .Where(json => !json.IsNullOrEmpty)
            .Select(json => JsonSerializer.Deserialize<ProductModel>((string)json!))
            .ToList();
        
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

    public async Task PatchAsync(int id,  ProductPatchFields updates) // consider refactoring
    {
        var product = await _sqlContext.Products.FindAsync(id);
        if (product == null)
        {
            await _cache.RemoveProductCacheAsync(id);
            return;
        }
        
        var json = await _redisDb.StringGetAsync($"product:{id}");
        var isCached = !json.IsNullOrEmpty;
        var modelForCacheUpdate = isCached ? JsonSerializer.Deserialize<ProductModel>((string)json!) : null;
        var batch = _redisDb.CreateBatch();

        if (!string.IsNullOrWhiteSpace(updates.Sku))
        {
            product.Sku = updates.Sku;
            if (isCached) modelForCacheUpdate?.Sku = updates.Sku;
        }

        if (!string.IsNullOrWhiteSpace(updates.Title))
        {
            if (isCached)
            {
                var oldTokens = SearchTokenizer.Tokenize(product.Title);
                foreach (var t in oldTokens)
                    _ = batch.SetRemoveAsync($"idx:word:{t}:products", id);
            }

            product.Title = updates.Title;
            if (isCached) modelForCacheUpdate?.Title = updates.Title;

            if (isCached)
            {
                var newTokens = SearchTokenizer.Tokenize(product.Title);
                foreach (var t in newTokens)
                    _ = batch.SetAddAsync($"idx:word:{t}:products", id);
            }
        }
        
        if (!string.IsNullOrWhiteSpace(updates.ShortDescription))
        {
            product.ShortDescription = updates.ShortDescription;
            if (isCached) modelForCacheUpdate?.Attributes?.ShortDescription = updates.ShortDescription;
        }
        
        if (updates.QuantityMin.HasValue)
        {
            product.QuantityMin = (int)updates.QuantityMin;
            if (isCached) modelForCacheUpdate?.Attributes?.QuantityMin = (int)updates.QuantityMin;
        }
        
        if (updates.QuantityMax.HasValue)
        {
            product.QuantityMax = (int)updates.QuantityMax;
            if (isCached) modelForCacheUpdate?.Attributes?.QuantityMax = (int)updates.QuantityMax;
        }
        
        if (updates.StartDate.HasValue)
        {
            product.StartDate = updates.StartDate;
            if (isCached) modelForCacheUpdate?.Attributes?.StartDate = updates.StartDate;
        }
        
        if (updates.EndDate.HasValue)
        {
            product.EndDate = updates.EndDate;
            if (isCached) modelForCacheUpdate?.Attributes?.EndDate = updates.EndDate;
        }
        
        if (updates.IsPromo.HasValue)
        {
            product.IsPromo = (bool)updates.IsPromo;
            if (isCached) modelForCacheUpdate?.Attributes?.IsPromo = (bool)updates.IsPromo;
        }
        
        if (updates.IsTop.HasValue)
        {
            product.IsTop = (bool)updates.IsTop;
            if (isCached) modelForCacheUpdate?.Attributes?.IsTop = (bool)updates.IsTop;
        }
        
        if (updates.IsNew.HasValue)
        {
            product.IsNew = (bool)updates.IsNew;
            if (isCached) modelForCacheUpdate?.Attributes?.IsNew = (bool)updates.IsNew;
        }
        
        if (!string.IsNullOrWhiteSpace(updates.Logo))
        {
            product.Logo = updates.Logo;
            if (isCached) modelForCacheUpdate?.Attributes?.Logo = updates.Logo;
        }
        
        if (!string.IsNullOrWhiteSpace(updates.Author))
        {
            product.Author = updates.Author;
            if (isCached) modelForCacheUpdate?.Attributes?.Author = updates.Author;
        }
        
        if (updates.VendorId.HasValue)
        {
            var newVendor = await _sqlContext.Vendors
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == updates.VendorId);
            if (newVendor != null)
            {
                if (isCached) _ = batch.SetRemoveAsync($"idx:vendor:{product.VendorId}:products", id);
                product.VendorId = newVendor.Id;
                if (isCached) modelForCacheUpdate?.Classification.Vendor = new VendorModel
                {
                    Id = newVendor.Id,
                    Name = newVendor.Name,
                    CountryCode = newVendor.CountryCode
                };
                if (isCached) _ = batch.SetAddAsync($"idx:vendor:{newVendor.Id}:products", id);
            }
        }
        
        if (updates.ProductTypeId.HasValue)
        {
            var newType = await _sqlContext.ProductTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == updates.ProductTypeId);
            if (newType != null)
            {
                if (isCached) _ = batch.SetRemoveAsync($"idx:product_type:{product.ProductTypeId}:products", id);
                product.ProductTypeId = newType.Id;
                if (isCached) modelForCacheUpdate?.Classification.TypeId = newType.Id;
                if (isCached) modelForCacheUpdate?.Classification.TypeName = newType.TypeName;
                if (isCached) _ = batch.SetAddAsync($"idx:product_type:{newType.Id}:products", id);
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
                if (isCached) modelForCacheUpdate?.Classification.UnitMeasureId = newUnitMeasure.Id;
                if (isCached) modelForCacheUpdate?.Classification.UnitMeasureName = newUnitMeasure.Name;
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
                if (isCached) modelForCacheUpdate?.Currency = new CurrencyModel()
                {
                    Id = newCurrency.Id,
                    Name = newCurrency.Name,
                    LiteralCode = newCurrency.LiteralCode
                };
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
                    _ = batch.SetRemoveAsync($"idx:group:{product.ProductGroupId}:products", id);
                    _ = batch.SetRemoveAsync($"idx:category:{oldGroup?.CategoryId}:products", id);
                }

                product.ProductGroupId = newGroup.Id;
                if (isCached) modelForCacheUpdate?.Classification.Group = new GroupModel()
                {
                    Id = newGroup.Id,
                    Name = newGroup.Name,
                    CategoryId = newGroup.CategoryId,
                    CategoryName = newGroup.Category.CategoryName
                };

                if (isCached)
                {
                    _ = batch.SetAddAsync($"idx:group:{newGroup.Id}:products", id);
                    _ = batch.SetAddAsync($"idx:category:{newGroup.CategoryId}:products", id);
                }
            }
        }

        await _sqlContext.SaveChangesAsync();
        if (isCached)
        {
            var updatedJson = JsonSerializer.Serialize(modelForCacheUpdate);
            _ = batch.StringSetAsync($"product:{id}", updatedJson);
            batch.Execute();
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
}