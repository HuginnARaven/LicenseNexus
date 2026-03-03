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
        var categoryIdVal = await _redisDb.HashGetAsync("pg_to_category_map", id);
        Category? groupCategory = null;
        
        if (!categoryIdVal.IsNull)
            groupCategory = await _redisDb.JSON().GetAsync<Category>($"category:{categoryIdVal}");
        
        var productGroup = groupCategory?.ProductGroups?.FirstOrDefault(g => g.Id == id);

        if (productGroup == null)
        {
            var dbProductGroup = await _context.ProductGroups
                .FirstOrDefaultAsync(p => p.Id == id);
    
            if (dbProductGroup != null)
            {
                productGroup = MapToRedisReadyProductGroup(dbProductGroup);
                
                var categoryKey = $"category:{dbProductGroup.CategoryId}";
                if (await _redisDb.KeyExistsAsync(categoryKey))
                {
                    await _redisDb.HashSetAsync("pg_to_category_map", dbProductGroup.Id.ToString(), dbProductGroup.CategoryId.ToString());
                    await _redisDb.JSON().ArrAppendAsync(categoryKey, "$.ProductGroups", productGroup);
                }
            }
        }
        return productGroup;
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        //return await _context.ProductGroups.AnyAsync(pg => pg.Id == id, cancellationToken);
        //return await _redisDb.KeyExistsAsync($"product_group:{id}");
        return await _redisDb.HashExistsAsync("pg_to_category_map", id);
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