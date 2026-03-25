using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Exceptions;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoProductGroupRepository: IProductGroupRepository
{
    private readonly MongoContext _context;
    
    public MongoProductGroupRepository(MongoContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<ProductGroup>> GetAllAsync()
    {
        var categories = await _context.Categories.Find(_ => true).ToListAsync();
        var productGroups = new List<ProductGroup>();

        foreach (var category in categories)
        {
            foreach (var groupDoc in category.Groups)
            {
                productGroups.Add(new ProductGroup
                {
                    Id = groupDoc.Id,
                    Name = groupDoc.Name,
                    IsActive = groupDoc.IsActive,
                    Note = groupDoc.Note,
                    CreatedDate = groupDoc.CreatedDate,
                    Author = groupDoc.Author,
                    CategoryId = category.Id,
                    Category = new Category
                    {
                        Id = category.Id,
                        CategoryName = category.Name,
                        IsActive = category.IsActive,
                        Description = category.Description,
                        CreatedDate = category.CreatedDate,
                        Author = groupDoc.Author
                    }
                });
            }
        }

        return productGroups;
    }

    public async Task<ProductGroup?> GetByIdAsync(int id)
    {
        var filter = Builders<CategoryDocument>.Filter.ElemMatch(c => c.Groups, g => g.Id == id);
        var category = await _context.Categories.Find(filter).FirstOrDefaultAsync();

        if (category == null)
        {
            return null;
        }

        var groupDoc = category.Groups.FirstOrDefault(g => g.Id == id);
        if (groupDoc == null)
        {
            return null;
        }

        return new ProductGroup
        {
            Id = groupDoc.Id,
            Name = groupDoc.Name,
            IsActive = groupDoc.IsActive,
            Note = groupDoc.Note,
            CreatedDate = groupDoc.CreatedDate,
            Author = groupDoc.Author,
            CategoryId = category.Id,
            Category = new Category
            {
                Id = category.Id,
                CategoryName = category.Name,
                IsActive = category.IsActive,
                Description = category.Description,
                CreatedDate = category.CreatedDate,
                Author = groupDoc.Author
            }
        };
    }
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<CategoryDocument>.Filter.ElemMatch(d => d.Groups, g => g.Id == id);
        return await _context.Categories.Find(filter).AnyAsync(cancellationToken);
    }

    public async Task<ProductGroup?> AddAsync(ProductGroup group)
    {
        var id = await _context.GetNextSequenceValueAsync("product_group_id");
        group.Id = id;

        var groupDoc = new ProductGroupDoc
        {
            Id = id,
            Name = group.Name,
            IsActive = group.IsActive,
            Note = group.Note,
            CreatedDate = DateTime.UtcNow,
            Author = group.Author
        };
        
        var filter = Builders<CategoryDocument>.Filter.And(
            Builders<CategoryDocument>.Filter.Eq(c => c.Id, group.CategoryId),
            Builders<CategoryDocument>.Filter.Not(
                Builders<CategoryDocument>.Filter.ElemMatch(c => c.Groups, g => g.Name == groupDoc.Name)
            )
        );
        var update = Builders<CategoryDocument>.Update.Push(c => c.Groups, groupDoc);
        var res = await _context.Categories.UpdateOneAsync(filter, update);
        
        if (res.MatchedCount == 0)
            throw new ConflictException($"Cannot add group. Either category with ID '{group.CategoryId}' does not exist, or a group with name '{group.Name}' already exists in this category.");
        
        return  group;
    }

    public async Task UpdateAsync(ProductGroup group)
    {
        var filter = Builders<CategoryDocument>.Filter.And(
            Builders<CategoryDocument>.Filter.ElemMatch(c => c.Groups, g => g.Id == group.Id),
            Builders<CategoryDocument>.Filter.Not(
                Builders<CategoryDocument>.Filter.ElemMatch(c => c.Groups, 
                    g => g.Name == group.Name && g.Id != group.Id)
            )
        );
        
        var update = Builders<CategoryDocument>.Update
            .Set("Groups.$.Name", group.Name)
            .Set("Groups.$.IsActive", group.IsActive)
            .Set("Groups.$.Note", group.Note)
            .Set("Groups.$.Author", group.Author);
        
        var res = await _context.Categories.UpdateOneAsync(filter, update);

        if (res.MatchedCount == 0)
            throw new ConflictException($"Cannot update. Either the group with Id '{group.Id}' doesn't exist, or a group with name '{group.Name}' already exists in this category.");
    }

    public async Task DeleteAsync(int id)
    {
        var filter = Builders<ProductDocument>.Filter.Eq(p => p.Classification.Group.Id, id);
        bool hasLinkedProducts = await _context.Products.Find(filter).AnyAsync();
        
        if (hasLinkedProducts)
            throw new ConflictException("A database constraint violation occurred. Cannot delete this object because it is assigned to one or more products.");
        
        var deleteFilter = Builders<CategoryDocument>.Filter.ElemMatch(c => c.Groups, g => g.Id == id);
        var update = Builders<CategoryDocument>.Update.PullFilter(c => c.Groups, g => g.Id == id);
        await _context.Categories.UpdateOneAsync(deleteFilter, update);
    }
}