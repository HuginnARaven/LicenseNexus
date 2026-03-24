using LicenseNexus.DataSeeder.Fakers;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.DataSeeder.Seeders;

public class TagSeeder
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly MongoContext _mongoContext;

    public TagSeeder(ExtendedSqlContext sqlContext, MongoContext mongoContext)
    {
        _sqlContext = sqlContext;
        _mongoContext = mongoContext;
    }

    public async Task SeedAsync(int totalTagsCount)
    {
        Console.WriteLine($"Starting to seed {totalTagsCount} tags...");

        var tagFaker = new TagFaker();
        
        var tags = tagFaker.GenerateUnique(totalTagsCount);
        await _sqlContext.Tags.AddRangeAsync(tags);
        await _sqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {tags.Count} tags to SQL.");

        var tagDocs = tags.Select(t => new TagDocument
        {
            Id = t.Id,
            Name = t.Name
        }).ToList();
        if (tagDocs.Any())
        {
            await _mongoContext.Tags.InsertManyAsync(tagDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "tag_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, tags.Max(с => с.Id)),
                new UpdateOptions { IsUpsert = true }
            );
            Console.WriteLine($"Saved {tagDocs.Count} tags to Mongo.");
        }
        
        Console.WriteLine("Finished seeding tags successfully.");
    }
}
    