using LicenseNexus.Domain.Entities;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.DataSeeder.Seeders;

public class CurrencySeeder
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly MongoContext _mongoContext;

    public CurrencySeeder(ExtendedSqlContext sqlContext, MongoContext mongoContext)
    {
        _sqlContext = sqlContext;
        _mongoContext = mongoContext;
    }

    public async Task SeedAsync()
    {
        Console.WriteLine($"Starting to seed 3 currencies...");

        var currencies = new List<Currency>
        {
            new Currency { LiteralCode = "USD", Name = "US Dollar", CountryCode = "USA" },
            new Currency { LiteralCode = "EUR", Name = "Euro", CountryCode = "EUR" },
            new Currency { LiteralCode = "UAH", Name = "Hryvnia", CountryCode = "UKR" }
        };
        
        await _sqlContext.Currencies.AddRangeAsync(currencies);
        await _sqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {currencies.Count} currencies to SQL.");

        var currencyDocs = currencies.Select(c => new CurrencyDocument
        {
            Id = c.Id,
            LiteralCode = c.LiteralCode,
            Name = c.Name,
            CountryCode = c.CountryCode
        }).ToList();
        if (currencyDocs.Any())
        {
            await _mongoContext.Currencies.InsertManyAsync(currencyDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "currency_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, currencies.Max(с => с.Id)),
                new UpdateOptions { IsUpsert = true }
            );
            Console.WriteLine($"Saved {currencyDocs.Count} currencies to Mongo.");
        }
        
        Console.WriteLine("Finished seeding currencies successfully.");
    }
}
    