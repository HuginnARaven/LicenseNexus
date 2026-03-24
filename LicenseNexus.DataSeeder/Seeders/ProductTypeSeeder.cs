using LicenseNexus.DataSeeder.Fakers;
using LicenseNexus.DataSeeder.Mappers;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;
using Tag = LicenseNexus.Domain.Entities.Tag;

namespace LicenseNexus.DataSeeder.Seeders;

public class ProductTypeSeeder
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly MongoContext _mongoContext;

    public ProductTypeSeeder(ExtendedSqlContext sqlContext, MongoContext mongoContext)
    {
        _sqlContext = sqlContext;
        _mongoContext = mongoContext;
    }

    public async Task SeedAsync()
    {
        Console.WriteLine($"Starting to seed 4 product types...");

        var productTypes = new List<ProductType>
        {
            new ProductType { TypeName = "License" },
            new ProductType { TypeName = "Subscription" },
            new ProductType { TypeName = "Service" },
            new ProductType { TypeName = "Physical Good" }
        };
        
        await _sqlContext.ProductTypes.AddRangeAsync(productTypes);
        await _sqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {productTypes.Count} product types to SQL.");

        var productTypeDocs = productTypes.Select(pt => new ProductTypeDocument
        {
            Id = pt.Id,
            TypeName = pt.TypeName
        }).ToList();
        if (productTypeDocs.Any())
        {
            await _mongoContext.ProductTypes.InsertManyAsync(productTypeDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "product_type_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, productTypes.Max(с => с.Id)),
                new UpdateOptions { IsUpsert = true }
            );
            Console.WriteLine($"Saved {productTypeDocs.Count} product types to Mongo.");
        }
        Console.WriteLine("Finished seeding product types successfully.");
    }
}
    