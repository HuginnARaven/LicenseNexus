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
            var product = await _sqlContext.Products
                .Include(p => p.Vendor) 
                .FirstOrDefaultAsync(p => p.Id == id);
        
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