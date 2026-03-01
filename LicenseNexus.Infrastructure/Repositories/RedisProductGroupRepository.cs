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
        var productGroup = await _redisDb.JSON().GetAsync<ProductGroup>($"product_group:{id}");
        if (productGroup == null)
        {
            var dbProductGroup = await _context.ProductGroups
                .Include(pg => pg.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
    
            if (dbProductGroup != null)
            {
                productGroup = MapToRedisReadyProductGroup(dbProductGroup, dbProductGroup.Category!);
                await _redisDb.JSON().SetAsync($"product_group:{productGroup.Id}", "$", productGroup);
            }
        }
        return productGroup;
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        //return await _context.ProductGroups.AnyAsync(pg => pg.Id == id, cancellationToken);
        return await _redisDb.KeyExistsAsync($"product_group:{id}");
    }
    
    public async Task<ProductGroup?> AddAsync(ProductGroup group)
    {
        _context.ProductGroups.Add(group);
        var res = await _context.SaveChangesAsync();
        if (res <= 0) return null;
        var productGroupCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == group.CategoryId);
        group = MapToRedisReadyProductGroup(group, productGroupCategory!);
        await _redisDb.JSON().SetAsync($"product_group:{group.Id}", "$", group);
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
        
        await _redisDb.JSON().MergeAsync($"product_group:{group.Id}", "$", updatePayload);
    }

    private ProductGroup MapToRedisReadyProductGroup(ProductGroup productGroup, Category category)
    {
        return new ProductGroup()
        {
            Id = productGroup.Id,
            Name = productGroup.Name,
            IsActive = productGroup.IsActive,
            Note = productGroup.Note,
            CreatedDate = productGroup.CreatedDate,
            Author = productGroup.Author,
            CategoryId = productGroup.CategoryId,
            Category = new Category
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                IsActive = category.IsActive,
                Description = category.Description,
                CreatedDate = category.CreatedDate,
                Author = category.Author
            }
        };
    }
}