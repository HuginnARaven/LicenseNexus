using LicenseNexus.DataSeeder.Fakers;
using LicenseNexus.DataSeeder.Mappers;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;
using Tag = LicenseNexus.Domain.Entities.Tag;

namespace LicenseNexus.DataSeeder.Seeders;

public class UnitMeasureSeeder
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly MongoContext _mongoContext;

    public UnitMeasureSeeder(ExtendedSqlContext sqlContext, MongoContext mongoContext)
    {
        _sqlContext = sqlContext;
        _mongoContext = mongoContext;
    }

    public async Task SeedAsync()
    {
        Console.WriteLine($"Starting to seed 4 unit measures...");

        var unitMeasures = new List<UnitMeasure>
        {
            new UnitMeasure { Name = "pcs" },
            new UnitMeasure { Name = "users" },
            new UnitMeasure { Name = "months" },
            new UnitMeasure { Name = "years" }
        };
        
        await _sqlContext.UnitMeasures.AddRangeAsync(unitMeasures);
        await _sqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {unitMeasures.Count} unit measures to SQL.");

        var unitMeasureDocs = unitMeasures.Select(um => new UnitMeasureDocument
        {
            Id = um.Id,
            Name = um.Name
        }).ToList();
        if (unitMeasureDocs.Any())
        {
            await _mongoContext.UnitMeasures.InsertManyAsync(unitMeasureDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "unit_measure_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, unitMeasures.Max(с => с.Id)),
                new UpdateOptions { IsUpsert = true }
            );
            Console.WriteLine($"Saved {unitMeasureDocs.Count} unit measures to Mongo.");
        }
        
        Console.WriteLine("Finished seeding unit measures successfully.");
    }
}
    