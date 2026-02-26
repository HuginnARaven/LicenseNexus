using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Exceptions;
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
            IsActive = d.IsActive,
            CategoryName = d.Name,
            Description = d.Description,
            CreatedDate = d.CreatedDate,
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
            
        return new Category
        {
            Id = doc.Id, 
            IsActive = doc.IsActive, 
            CategoryName = doc.Name, 
            Description = doc.Description,
            CreatedDate = doc.CreatedDate,
            ProductGroups = doc.Groups.Select(g => new ProductGroup
            {
                Id = g.Id,
                Name = g.Name,
                IsActive = g.IsActive,
                CategoryId = doc.Id
            }).ToList()
        };
    }

    public async Task<Category?> AddAsync(Category category)
    {
        var id = await _context.GetNextSequenceValueAsync("category_id");
        category.Id = id;

        var doc = new CategoryDocument
        {
            Id = id,
            IsActive = category.IsActive,
            Name = category.CategoryName,
            Description = category.Description,
        };

        await _context.Categories.InsertOneAsync(doc);
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        var filter = Builders<CategoryDocument>.Filter.Eq(c => c.Id, category.Id);
        var update = Builders<CategoryDocument>.Update
            .Set(c => c.Name, category.CategoryName)
            .Set(c => c.IsActive, category.IsActive)
            .Set(c => c.Description, category.Description);
            
        await _context.Categories.UpdateOneAsync(filter, update);
    }
    
    public async Task DeleteAsync(int id)
    {
        var categoryProjection = await _context.Categories
            .Find(c => c.Id == id)
            .Project(c => new { GroupIds = c.Groups.Select(g => g.Id) })
            .FirstOrDefaultAsync();
        
        if (categoryProjection != null && categoryProjection.GroupIds.Any())
        {
            var productFilter = Builders<ProductDocument>.Filter.In(p => p.Classification.Group.Id, categoryProjection.GroupIds);
        
            bool hasLinkedProducts = await _context.Products.Find(productFilter).AnyAsync();

            if (hasLinkedProducts)
                throw new ConflictException("A database constraint violation occurred. Cannot delete this category because one or more of its product groups are assigned to products.");
        }

        var deleteFilter = Builders<CategoryDocument>.Filter.Eq(c => c.Id, id);
        await _context.Categories.DeleteOneAsync(deleteFilter);
    }
}