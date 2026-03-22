using LicenseNexus.DataSeeder.Fakers;
using LicenseNexus.DataSeeder.Mappers;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;
using Tag = LicenseNexus.Domain.Entities.Tag;

namespace LicenseNexus.DataSeeder.Seeders;

public class ProductSeeder
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly MongoContext _mongoContext;

    public ProductSeeder(ExtendedSqlContext sqlContext, MongoContext mongoContext)
    {
        _sqlContext = sqlContext;
        _mongoContext = mongoContext;
    }

    public async Task SeedAsync(
        int totalProductsCount,
        int batchSize,
        List<Vendor> vendors,
        List<ProductType> types,
        List<UnitMeasure> measures,
        List<Currency> currencies,
        List<ProductGroup> groups,
        List<Tag> tags)
    {
        Console.WriteLine($"Starting to seed {totalProductsCount} products in batches of {batchSize}...");
        
        var productFaker = new ProductFaker(vendors, types, measures, currencies, groups);
        var priceFaker = new ProductPriceFaker();
        var descriptionFaker = new FullDescriptionFaker();
        var productTagFaker = new ProductTagFaker(tags);
        
        int maxProductId = 0;
        int maxPriceId = 0;
        int maxDescriptionId = 0;

        int totalBatches = (int)Math.Ceiling((double)totalProductsCount / batchSize);

        for (int i = 0; i < totalProductsCount; i += batchSize)
        {
            int currentBatchSize = Math.Min(batchSize, totalProductsCount - i);
            
            var productsBatch = productFaker.Generate(currentBatchSize);
            
            await _sqlContext.Products.AddRangeAsync(productsBatch);
            await _sqlContext.SaveChangesAsync();

            var pricesBatch = new List<ProductPrice>();
            var descriptionsBatch = new List<FullDescription>();
            var productTagsBatch = new List<ProductTag>();
            
            foreach (var product in productsBatch)
            {
                var prices = priceFaker.GenerateForProduct(product.Id);
                var descriptions = descriptionFaker.GenerateForProduct(product.Id);
                var productTags = productTagFaker.GenerateForProduct(product.Id, 4);

                pricesBatch.AddRange(prices);
                descriptionsBatch.AddRange(descriptions);
                productTagsBatch.AddRange(productTags);
                
                product.Prices = prices;
                product.FullDescriptions = descriptions;
                product.ProductTags = productTags;
            }
            
            await _sqlContext.ProductPrices.AddRangeAsync(pricesBatch);
            await _sqlContext.FullDescriptions.AddRangeAsync(descriptionsBatch);
            await _sqlContext.ProductTags.AddRangeAsync(productTagsBatch);
            await _sqlContext.SaveChangesAsync();
            
            var productDocs = productsBatch.Select(p => ProductDocumentMapper.Map(
                p, tags, groups, vendors, types, measures, currencies)
            ).ToList();

            if (productDocs.Any())
            {
                await _mongoContext.Products.InsertManyAsync(productDocs);
            }
            
            if (productsBatch.Any()) maxProductId = Math.Max(maxProductId, productsBatch.Max(p => p.Id));
            if (pricesBatch.Any()) maxPriceId = Math.Max(maxPriceId, pricesBatch.Max(p => p.Id));
            if (descriptionsBatch.Any()) maxDescriptionId = Math.Max(maxDescriptionId, descriptionsBatch.Max(d => d.Id));
            
            _sqlContext.ChangeTracker.Clear();

            Console.WriteLine($"Processed batch {(i / batchSize) + 1} / {totalBatches}. Products saved: {i + currentBatchSize}");
        }
        
        await UpdateMongoCountersAsync(maxProductId, maxPriceId, maxDescriptionId);
        Console.WriteLine("Finished seeding products successfully.");
    }

    private async Task UpdateMongoCountersAsync(int maxProductId, int maxPriceId, int maxDescriptionId)
    {
        if (maxProductId > 0)
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "product_id", Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, maxProductId),
                new UpdateOptions { IsUpsert = true });

        if (maxPriceId > 0)
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "product_price_id", Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, maxPriceId),
                new UpdateOptions { IsUpsert = true });

        if (maxDescriptionId > 0)
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "product_description_id", Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, maxDescriptionId),
                new UpdateOptions { IsUpsert = true });
    }
}