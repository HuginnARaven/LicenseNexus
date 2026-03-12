using System.Text.Json;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisProductGroupRepository: IProductGroupRepository
{
    private readonly ExtendedSqlContext _context;
    private readonly IDatabase _redisDb;
    
    public RedisProductGroupRepository(ExtendedSqlContext context, RedisContext redisContext)
    {
        _context = context;
        _redisDb = redisContext.Database;
    }
    
    public async Task<IEnumerable<ProductGroup>> GetAllAsync()
    {
        return await _context.ProductGroups.ToListAsync();
    }

    public async Task<ProductGroup?> GetByIdAsync(int id)
    {
        var negativeCacheKey = $"product_group:{id}:notfound";
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return null;
        
        var categoryIdVal = await _redisDb.HashGetAsync("pg_to_category_map", id);
        if (!categoryIdVal.IsNull)
        {
            var groupCategory = await _redisDb.JSON().GetAsync<Category>($"category:{categoryIdVal}");
            var cachedProductGroup = groupCategory?.ProductGroups?.FirstOrDefault(g => g.Id == id);
            if (cachedProductGroup != null)
                return cachedProductGroup;
        }
        
        var dbProductGroup = await _context.ProductGroups.FirstOrDefaultAsync(p => p.Id == id);
        if (dbProductGroup == null)
        {
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));
            return null;
        }
        
        var productGroup = MapToRedisReadyProductGroup(dbProductGroup);
        var categoryKey = $"category:{dbProductGroup.CategoryId}";
        if (await _redisDb.KeyExistsAsync(categoryKey))
        {
            await _redisDb.HashSetAsync("pg_to_category_map", dbProductGroup.Id.ToString(), dbProductGroup.CategoryId.ToString());
            await _redisDb.JSON().ArrAppendAsync(categoryKey, "$.ProductGroups", productGroup);
        }
        else //TODO: make separate method CacheCategoryByIdAsync in cache service
        {
            var dbCategory = await _context.Categories
                .Include(c => c.ProductGroups)
                .FirstOrDefaultAsync(c => c.Id == dbProductGroup.CategoryId);
            if (dbCategory != null)
            {
                var redisCategory = new Category() {
                    Id = dbCategory.Id,
                    CategoryName = dbCategory.CategoryName,
                    IsActive = dbCategory.IsActive,
                    Description = dbCategory.Description,
                    CreatedDate = dbCategory.CreatedDate,
                    Author = dbCategory.Author,
                    ProductGroups = dbCategory.ProductGroups.Select(pg => new ProductGroup
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
                await _redisDb.JSON().SetAsync(categoryKey, "$", redisCategory);
                var hashEntries = dbCategory.ProductGroups
                    .Select(g => new HashEntry(g.Id.ToString(), dbCategory.Id.ToString()))
                    .ToArray();
                if (hashEntries.Any())
                    await _redisDb.HashSetAsync("pg_to_category_map", hashEntries);
            }
        }
        return productGroup;
    }
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var hashKey = "pg_to_category_map";
        var negativeCacheKey = $"product_group:{id}:notfound";
        
        if (await _redisDb.HashExistsAsync(hashKey, id.ToString()))
            return true;
        
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return false;
        
        var existsInDb = await _context.ProductGroups.AnyAsync(pg => pg.Id == id, cancellationToken);

        if (!existsInDb)
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));
        
        return existsInDb;
    }
    
    public async Task<ProductGroup?> AddAsync(ProductGroup group)
    {
        _context.ProductGroups.Add(group);
        var res = await _context.SaveChangesAsync();
        if (res <= 0) return null;
       
        group = MapToRedisReadyProductGroup(group);
        
        var categoryKey = $"category:{group.CategoryId}";
        if (await _redisDb.KeyExistsAsync(categoryKey))
        {
            await _redisDb.HashSetAsync("pg_to_category_map", group.Id.ToString(), group.CategoryId.ToString());
            await _redisDb.JSON().ArrAppendAsync(categoryKey, "$.ProductGroups", group);
        }
        
        return group;
    }

    public async Task UpdateAsync(ProductGroup group)
    {
        await _context.ProductGroups.Where(g => g.Id == group.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(g => g.Name, group.Name)
            .SetProperty(g => g.IsActive, group.IsActive)
            .SetProperty(g => g.Note, group.Note)
            .SetProperty(g => g.Author, group.Author)
        );
        
        var updatePayload = new 
        {
            Name = group.Name,
            IsActive = group.IsActive,
            Note = group.Note,
            Author = group.Author
        };
        
        var categoryIdVal = await _redisDb.HashGetAsync("pg_to_category_map", group.Id);
        if (!categoryIdVal.IsNull)
            await _redisDb.JSON().MergeAsync($"category:{categoryIdVal}", $"$.ProductGroups[?(@.Id=={group.Id})]", updatePayload);
    }

    private ProductGroup MapToRedisReadyProductGroup(ProductGroup productGroup)
    {
        return new ProductGroup()
        {
            Id = productGroup.Id,
            Name = productGroup.Name,
            IsActive = productGroup.IsActive,
            Note = productGroup.Note,
            CreatedDate = productGroup.CreatedDate,
            Author = productGroup.Author,
            CategoryId = productGroup.CategoryId
        };
    }
}