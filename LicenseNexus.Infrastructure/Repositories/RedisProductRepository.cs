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
using NRedisStack.Search;
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
        var redisKey = $"product:{id}";
        var negativeCacheKey = $"product:{id}:notfound";
        
        var product = await _redisDb.JSON().GetAsync<ProductModel>(redisKey);
        if (product != null)
            return product;
        
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return null;
        
        var existsInDb = await _sqlContext.Products.AnyAsync(p => p.Id == id);
        if (!existsInDb)
        {
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));
            return null;
        }
        
        await _cache.CacheProductByIdAsync(id);
        return await _cache.CacheProductByIdAsync(id);
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
        var redisKey = $"product:{id}";
        var negativeCacheKey = $"product:{id}:notfound";
        
        if (await _redisDb.KeyExistsAsync(redisKey))
            return true;
        
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return false;
        
        var existsInDb = await _sqlContext.Products.AnyAsync(p => p.Id == id, cancellationToken);

        if (!existsInDb)
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));
        
        return existsInDb;
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
        // var rawKeyName = $"{page}|{pageSize}|{categoryId}|{groupId}|{vendorId}|{search}|{priceFrom}|{priceTo}";
        //
        // using var sha256 = System.Security.Cryptography.SHA256.Create();
        // var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawKeyName));
        // var hashString = Convert.ToBase64String(hashBytes).Replace("/", "_").Replace("+", "-").Substring(0, 20);
        //
        // var cacheKey = $"search:req:{hashString}";
        //
        // var cachedResult = await _redisDb.StringGetAsync(cacheKey);
        // if (cachedResult.HasValue)
        // {
        //     return JsonSerializer.Deserialize<PaginatedResult<ProductModel>>((string)cachedResult!)!;
        // }
        
        var skip = (page - 1) * pageSize;
        var take = pageSize;
        
        var queryParts = new List<string>();
        if (categoryId.HasValue) queryParts.Add($"@CategoryId:[{categoryId.Value} {categoryId.Value}]");
        if (groupId.HasValue) queryParts.Add($"@GroupId:[{groupId.Value} {groupId.Value}]");
        if (vendorId.HasValue) queryParts.Add($"@VendorId:[{vendorId.Value} {vendorId.Value}]");

        if (priceFrom.HasValue || priceTo.HasValue)
        {
            string minPrice = priceFrom.HasValue ? priceFrom.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "-inf";
            string maxPrice = priceTo.HasValue ? priceTo.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "+inf";
            queryParts.Add($"@Price:[{minPrice} {maxPrice}]");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var cleanSearch = search.Trim().Replace("-", "\\-"); 
            queryParts.Add($"@Title:({cleanSearch})");
        }

        string queryString = queryParts.Count > 0 ? string.Join(" ", queryParts) : "*";

        var query = new Query(queryString)
            .Limit(skip, take)
            .Dialect(2);

        var searchResult = await _redisDb.FT().SearchAsync("idx:products", query);
        var products = new List<ProductModel>();
        foreach (var doc in searchResult.Documents)
        {
            var json = doc["json"];
            if (!string.IsNullOrEmpty(json))
            {
                var product = JsonSerializer.Deserialize<ProductModel>((string)json!);
                if (product != null) products.Add(product);
            }
        }

        var result = new PaginatedResult<ProductModel>
        {
            Items = products,
            TotalCount = searchResult.TotalResults,
            Page = page,
            PageSize = pageSize
        };

        // var serializedResult = JsonSerializer.Serialize(result);
        // await _redisDb.StringSetAsync(cacheKey, serializedResult, TimeSpan.FromSeconds(60));

        return result;
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
            product.Title = updates.Title;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Title", JsonSerializer.Serialize(updates.Title)));
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
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.StartDate", $"\"{updates.StartDate.Value:O}\""));
        }
        
        if (updates.EndDate.HasValue)
        {
            product.EndDate = updates.EndDate;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.EndDate", $"\"{updates.EndDate.Value:O}\""));
        }
        
        if (updates.IsPromo.HasValue)
        {
            product.IsPromo = updates.IsPromo.Value;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.IsPromo", updates.IsPromo.Value ? "true" : "false"));
        }
        
        if (updates.IsTop.HasValue)
        {
            product.IsTop = updates.IsTop.Value;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.IsTop", updates.IsTop.Value ? "true" : "false"));
        }
        
        if (updates.IsNew.HasValue)
        {
            product.IsNew = updates.IsNew.Value;
            if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Attributes.IsNew", updates.IsNew.Value ? "true" : "false"));
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
            var newVendor = await _redisDb.JSON().GetAsync<Vendor>($"vendor:{updates.VendorId}") ?? 
                            await _sqlContext.Vendors.AsNoTracking().FirstOrDefaultAsync(e => e.Id == updates.VendorId);
            if (newVendor != null)
            {
                product.VendorId = newVendor.Id;
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.Vendor",  new VendorModel
                {
                    Id = newVendor.Id,
                    Name = newVendor.Name,
                    CountryCode = newVendor.CountryCode
                }));
            }
        }
        
        if (updates.ProductTypeId.HasValue)
        {
            var newType = await _redisDb.JSON().GetAsync<ProductType>($"product_type:{updates.ProductTypeId}") ?? 
                          await _sqlContext.ProductTypes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == updates.ProductTypeId);
            if (newType != null)
            {
                product.ProductTypeId = newType.Id;
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.TypeId", newType.Id));
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.TypeName", $"\"{newType.TypeName}\""));
            }
        }
        
        if (updates.UnitMeasureId.HasValue)
        {
            var newUnitMeasure = await _redisDb.JSON().GetAsync<UnitMeasure>($"unit_measure:{updates.UnitMeasureId}") ?? 
                                 await _sqlContext.UnitMeasures.AsNoTracking().FirstOrDefaultAsync(e => e.Id == updates.UnitMeasureId);
            if (newUnitMeasure != null)
            {
                product.UnitMeasureId = newUnitMeasure.Id;
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.UnitMeasureId", newUnitMeasure.Id));
                if (isCached) cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.UnitMeasureName", $"\"{newUnitMeasure.Name}\""));
            }
        }
        
        if (updates.CurrencyId.HasValue)
        {
            var newCurrency = await _redisDb.JSON().GetAsync<Currency>($"currency:{updates.CurrencyId}") ?? 
                              await _sqlContext.Currencies.AsNoTracking().FirstOrDefaultAsync(e => e.Id == updates.CurrencyId);
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
            ProductGroup? newGroup = null;
            Category? newCategory = null;

            var categoryIdVal = await _redisDb.HashGetAsync("pg_to_category_map", updates.ProductGroupId.Value);
            if (categoryIdVal.HasValue)
            {
                newCategory = await _redisDb.JSON().GetAsync<Category>($"category:{categoryIdVal}");
                newGroup = newCategory?.ProductGroups?.FirstOrDefault(g => g.Id == updates.ProductGroupId);
            }
            
            if (newGroup == null || newCategory == null)
            {
                newGroup = await _sqlContext.ProductGroups
                    .AsNoTracking()
                    .Include(e => e.Category)
                    .FirstOrDefaultAsync(e => e.Id == updates.ProductGroupId);
                newCategory = newGroup?.Category;
            }
            
            if (newGroup != null && newCategory != null)
            {
                var oldGroupId = product.ProductGroupId;
                product.ProductGroupId = newGroup.Id;
                
                if (isCached)
                {
                    var oldCategoryIdVal = await _redisDb.HashGetAsync("pg_to_category_map", oldGroupId);
                    var oldCategoryId = oldCategoryIdVal.HasValue 
                        ? (int)oldCategoryIdVal 
                        : await _sqlContext.ProductGroups
                            .AsNoTracking()
                            .Where(e => e.Id == oldGroupId)
                            .Select(e => e.CategoryId)
                            .FirstOrDefaultAsync(); ;
                    
                    cacheTasks.Add(pipeline!.Json.SetAsync(productKey, "$.Classification.Group",  new GroupModel()
                    {
                        Id = newGroup.Id,
                        Name = newGroup.Name,
                        CategoryId = newCategory.Id, 
                        CategoryName = newCategory.CategoryName 
                    }));
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

    public async Task<ProductPriceModel?> GetPriceAsync(int productId, int priceId)
    {
        var price = await _redisDb.JSON().GetAsync<ProductPriceModel>($"product:{productId}", $"$.Prices[?(@.Id=={priceId})]");
        if (price == null)
        {
            var dbPrice = await _sqlContext.ProductPrices.FirstOrDefaultAsync(p => p.Id == priceId && p.ProductId == productId);
            if (dbPrice == null) return null;
            price = new ProductPriceModel {
                Id = dbPrice.Id,
                Price = dbPrice.Price,
                TermDuration = dbPrice.TermDuration,
                BillingPlan = dbPrice.BillingPlan,
                CountryCode = dbPrice.CountryCode,
                Segment = dbPrice.Segment,
                StartDate = dbPrice.StartDate
            };
            await _redisDb.JSON().ArrAppendAsync($"product:{productId}", "$.Prices", price);
        }
        return price;
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