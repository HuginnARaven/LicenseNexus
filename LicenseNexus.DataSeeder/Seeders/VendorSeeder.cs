using LicenseNexus.DataSeeder.Fakers;
using LicenseNexus.DataSeeder.Mappers;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;
using Tag = LicenseNexus.Domain.Entities.Tag;

namespace LicenseNexus.DataSeeder.Seeders;

public class VendorSeeder
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly MongoContext _mongoContext;

    public VendorSeeder(ExtendedSqlContext sqlContext, MongoContext mongoContext)
    {
        _sqlContext = sqlContext;
        _mongoContext = mongoContext;
    }

    public async Task SeedAsync(int totalVendorsCount)
    {
        Console.WriteLine($"Starting to seed {totalVendorsCount} vendors...");

        var vendorFaker = new VendorFaker();
        var vendors = vendorFaker.Generate(totalVendorsCount);
        await _sqlContext.Vendors.AddRangeAsync(vendors);
        await _sqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {vendors.Count} vendors to SQL.");

        var vendorDocs = vendors.Select(v => new VendorDocument
        {
            Id = v.Id,
            Name = v.Name,
            OriginalName = v.OriginalName,
            Description = v.Description,
            CountryCode = v.CountryCode,
            Logo = v.Logo
        }).ToList();
        if (vendorDocs.Any())
        {
            await _mongoContext.Vendors.InsertManyAsync(vendorDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "vendor_id",
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, vendors.Max(v => v.Id)),
                new UpdateOptions { IsUpsert = true });
            Console.WriteLine($"Saved {vendorDocs.Count} vendors to Mongo.");
        }

        Console.WriteLine("Finished seeding vendors successfully.");
    }
}
    