using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoCategoryRepository: ICategoryRepository
{
    private readonly MongoContext _context;
    
    public MongoCategoryRepository(MongoContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        var docs = await _context.Categories.Find(_ => true).ToListAsync();
        return docs.Select(d => new Category 
        { 
            Id = d.Id, 
            CategoryName = d.Name,
            IsActive = d.IsActive,
            ProductGroups = d.Groups.Select(g => new ProductGroup
            {
                Id = g.Id,
                Name = g.Name,
                IsActive = g.IsActive,
                CategoryId = d.Id
            }).ToList()
        });
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        var doc = await _context.Categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (doc == null) return null;
            
        return new Category { Id = doc.Id, CategoryName = doc.Name, IsActive = doc.IsActive };
    }

    public async Task AddAsync(Category category)
    {
        var id = await _context.GetNextSequenceValueAsync("category_id");
        category.Id = id;

        var doc = new CategoryDocument
        {
            Id = id,
            Name = category.CategoryName,
            IsActive = category.IsActive
        };

        await _context.Categories.InsertOneAsync(doc);
    }

    public async Task AddGroupAsync(int categoryId, ProductGroup group)
    {
        var groupId = await _context.GetNextSequenceValueAsync("product_group_id");
        group.Id = groupId;
        
        var groupDoc = new ProductGroupDoc
        {
            Id = groupId,
            Name = group.Name,
            IsActive = group.IsActive
        };
        
        var filter = Builders<CategoryDocument>.Filter.Eq(c => c.Id, categoryId);
        var update = Builders<CategoryDocument>.Update.Push(c => c.Groups, groupDoc);

        await _context.Categories.UpdateOneAsync(filter, update);
    }
}