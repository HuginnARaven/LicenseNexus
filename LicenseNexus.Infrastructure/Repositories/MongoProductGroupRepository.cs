using LicenseNexus.Domain.Entities;
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
                    CategoryId = category.Id
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
            CategoryId = category.Id
        };
    }

    public async Task AddAsync(ProductGroup group)
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

        var categoryFilter = Builders<CategoryDocument>.Filter.Eq(c => c.Id, group.CategoryId);
        var uniquenessFilter = Builders<CategoryDocument>.Filter.Not(Builders<CategoryDocument>.Filter.ElemMatch(c => c.Groups, g => g.Name == groupDoc.Name));
        var filter = Builders<CategoryDocument>.Filter.And(categoryFilter, uniquenessFilter);
        var update = Builders<CategoryDocument>.Update.Push(c => c.Groups, groupDoc);
        // TODO: add throw validation error if not created
        await _context.Categories.UpdateOneAsync(filter, update);
    }

    public async Task UpdateAsync(ProductGroup group)
    {
        var filter = Builders<CategoryDocument>.Filter.ElemMatch(c => c.Groups, g => g.Id == group.Id);
        var update = Builders<CategoryDocument>.Update
            .Set("Groups.$.Name", group.Name)
            .Set("Groups.$.IsActive", group.IsActive)
            .Set("Groups.$.Note", group.Note)
            .Set("Groups.$.Author", group.Author);

        await _context.Categories.UpdateOneAsync(filter, update);
    }
}