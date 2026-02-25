using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoTagRepository(MongoContext context): ITagRepository
{
    public async Task<IEnumerable<Domain.Entities.Tag>> GetAllAsync()
    {
        var docs = await context.Tags.Find(_ => true).ToListAsync();
        return docs.Select(doc => new Domain.Entities.Tag
        { 
            Id = doc.Id, 
            Name = doc.Name
        });
    }

    public async Task<Domain.Entities.Tag?> GetByIdAsync(int id)
    {
        var doc = await context.Tags.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (doc == null) return null;
            
        return new Domain.Entities.Tag 
        { 
            Id = doc.Id, 
            Name = doc.Name
        };
    }
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TagDocument>.Filter.Eq(d => d.Id, id);
        return await context.Tags.Find(filter).AnyAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Tag?> AddAsync(Domain.Entities.Tag tag)
    {
        var id = await context.GetNextSequenceValueAsync("tag_id");
        tag.Id = id;

        var doc = new TagDocument() 
        {
            Id = id,
            Name = tag.Name
        };

        await context.Tags.InsertOneAsync(doc);
        return tag;
    }

    public async Task UpdateAsync(Domain.Entities.Tag tag)
    {
        var filter = Builders<TagDocument>.Filter.Eq(t => t.Id, tag.Id);
        var update = Builders<TagDocument>.Update
            .Set(t => t.Name, tag.Name);

        await context.Tags.UpdateOneAsync(filter, update);
    }

    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}