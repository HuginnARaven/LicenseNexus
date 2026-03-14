using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisCategoryRepository: ICategoryRepository
{
    private readonly ExtendedSqlContext _context;
    private readonly IDatabase _redisDb;

    public RedisCategoryRepository(ExtendedSqlContext context, RedisContext redisContext)
    {
        _context = context;
        _redisDb = redisContext.Database;
    }
    
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.Include(c => c.ProductGroups).Where(_ => true).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        var negativeCacheKey = $"category:{id}:notfound";
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return null;
        
        var category = await _redisDb.JSON().GetAsync<Category>($"category:{id}");
        if (category != null)
            return category;
        

        var dbCategory = await _context.Categories.Include(c => c.ProductGroups).FirstOrDefaultAsync(c => c.Id == id);
        if (dbCategory == null)
        {
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));
            return null;
        }

        var redisCategory = MapToRedisReadyCategory(dbCategory);
        await _redisDb.JSON().SetAsync($"category:{id}", "$", redisCategory);
        
        var hashEntries = dbCategory.ProductGroups
            .Select(g => new HashEntry(g.Id.ToString(), dbCategory.Id.ToString()))
            .ToArray();
        if (hashEntries.Length != 0)
            await _redisDb.HashSetAsync("pg_to_category_map", hashEntries);
        
        return redisCategory;
    }

    public async Task<Category?> AddAsync(Category category)
    {
        _context.Categories.Add(category);
        var res = await _context.SaveChangesAsync();
        if (res > 0)
        {
            var redisCategory = MapToRedisReadyCategory(category);
            await _redisDb.JSON().SetAsync($"category:{category.Id}", "$", redisCategory);
            return category;
        }
        
        return null;
    }

    public async Task UpdateAsync(Category category)
    {
        await _context.Categories.Where(c => c.Id == category.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(c => c.CategoryName, category.CategoryName)
            .SetProperty(c => c.IsActive, category.IsActive)
            .SetProperty(c => c.Description, category.Description)
            .SetProperty(c => c.Author, category.Author)
        );
        
        var updatePayload = new 
        {
            CategoryName = category.CategoryName,
            IsActive = category.IsActive,
            Description = category.Description,
            Author = category.Author
        };
        
        await _redisDb.JSON().MergeAsync($"category:{category.Id}", "$", updatePayload);
    }

    public async Task DeleteAsync(int id)
    {
        var category = new Category { Id = id };
        _context.Categories.Attach(category);
        _context.Categories.Remove(category);
  
        await _context.SaveChangesAsync();
        await _redisDb.KeyDeleteAsync($"category:{id}");
        await _redisDb.HashDeleteAsync("pg_to_category_map", id.ToString());
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var hashKey = "pg_to_category_map";
        var negativeCacheKey = $"category:{id}:notfound";
        
        if (await _redisDb.HashExistsAsync(hashKey, id.ToString()))
            return true;
        
        if (await _redisDb.KeyExistsAsync(negativeCacheKey))
            return false;
        
        var existsInDb = await _context.Categories.AnyAsync(c => c.Id == id, cancellationToken);

        if (!existsInDb)
            await _redisDb.StringSetAsync(negativeCacheKey, "1", TimeSpan.FromMinutes(5));

        return existsInDb;
    }

    private Category MapToRedisReadyCategory(Category category)
    {
        return new Category()
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
    }
}