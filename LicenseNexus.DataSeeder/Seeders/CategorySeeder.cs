using LicenseNexus.DataSeeder.Fakers;
using LicenseNexus.DataSeeder.Mappers;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;
using Tag = LicenseNexus.Domain.Entities.Tag;

namespace LicenseNexus.DataSeeder.Seeders;

public class CategorySeeder
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly MongoContext _mongoContext;

    public CategorySeeder(ExtendedSqlContext sqlContext, MongoContext mongoContext)
    {
        _sqlContext = sqlContext;
        _mongoContext = mongoContext;
    }

    public async Task SeedAsync(int totalCategoriesCount, int groupsPerCategoryCount)
    {
        Console.WriteLine($"Starting to seed {totalCategoriesCount} categories...");

        var categoryFaker = new CategoryFaker();
        var productGroupsFaker = new ProductGroupsFaker();
        
        var categories = categoryFaker.Generate(totalCategoriesCount);
        await _sqlContext.Categories.AddRangeAsync(categories);
        await _sqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {categories.Count} categories to SQL.");

        var categoryDocs = new List<CategoryDocument>();
        var maxProductGroupId = 0;
        
        foreach (var cat in categories)
        {
            var groups = productGroupsFaker.GenerateForCategory(cat.Id, groupsPerCategoryCount);
            await _sqlContext.ProductGroups.AddRangeAsync(groups);
            await _sqlContext.SaveChangesAsync(); // Save groups to get IDs
            
            cat.ProductGroups = groups; // Link for local usage if needed

            var groupDocs = groups.Select(g => new ProductGroupDoc
            {
                Id = g.Id,
                Name = g.Name,
                IsActive = g.IsActive,
                Note = g.Note,
                CreatedDate = g.CreatedDate,
                Author = g.Author
            }).ToList();

            categoryDocs.Add(new CategoryDocument
            {
                Id = cat.Id,
                IsActive = cat.IsActive,
                Name = cat.CategoryName,
                Description = cat.Description,
                CreatedDate = cat.CreatedDate,
                Author = cat.Author,
                Groups = groupDocs
            });
            
            maxProductGroupId = Math.Max(maxProductGroupId, groups.Any() ? groups.Max(pg => pg.Id) : 0);
        }
        if (categoryDocs.Any())
        {
            await _mongoContext.Categories.InsertManyAsync(categoryDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "category_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, categories.Max(с => с.Id)),
                new UpdateOptions { IsUpsert = true }
            );
            if (maxProductGroupId > 0)
                await _mongoContext.Counters.UpdateOneAsync(
                    ds => ds.Id == "product_group_id", 
                    Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, maxProductGroupId),
                    new UpdateOptions { IsUpsert = true }
                );
            Console.WriteLine($"Saved {categoryDocs.Count} categories to Mongo.");
        }

        Console.WriteLine("Finished seeding categories successfully.");
    }
}
    