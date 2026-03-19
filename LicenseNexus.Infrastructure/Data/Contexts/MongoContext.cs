using LicenseNexus.Infrastructure.Data.MongoDocuments;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Data.Contexts;

public class MongoContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }
    
    public IMongoCollection<ProductDocument> Products => _database.GetCollection<ProductDocument>(_settings.CollectionName);
    public IMongoCollection<CartDocument> Carts => _database.GetCollection<CartDocument>("Carts");
    public IMongoCollection<DatabaseSequence> Counters => _database.GetCollection<DatabaseSequence>("Counters");
    public IMongoCollection<CategoryDocument> Categories => _database.GetCollection<CategoryDocument>("Categories");
    public IMongoCollection<VendorDocument> Vendors => _database.GetCollection<VendorDocument>("Vendors");
    public IMongoCollection<ProductTypeDocument> ProductTypes => _database.GetCollection<ProductTypeDocument>("ProductTypes");
    public IMongoCollection<UnitMeasureDocument> UnitMeasures => _database.GetCollection<UnitMeasureDocument>("UnitMeasures");
    public IMongoCollection<CurrencyDocument> Currencies => _database.GetCollection<CurrencyDocument>("Currencies");
    public IMongoCollection<TagDocument> Tags => _database.GetCollection<TagDocument>("Tags");
    
    public async Task<int> GetNextSequenceValueAsync(string sequenceName)
    {
        var filter = Builders<DatabaseSequence>.Filter.Eq(a => a.Id, sequenceName);
        var update = Builders<DatabaseSequence>.Update.Inc(a => a.Seq, 1);
        var options = new FindOneAndUpdateOptions<DatabaseSequence>
        {
            ReturnDocument = ReturnDocument.After,
            IsUpsert = true
        };

        var result = await Counters.FindOneAndUpdateAsync(filter, update, options);
        return result.Seq;
    }
    
    public async Task ConfigureIndexesAsync()
    {
        await Products.Indexes.DropAllAsync();
        
        // Products
        // var productIndexes = new List<CreateIndexModel<ProductDocument>>
        // {
        //     new CreateIndexModel<ProductDocument>(
        //         Builders<ProductDocument>.IndexKeys.Ascending(p => p.ProductId),
        //         new CreateIndexOptions { Unique = true }
        //     ),
        //     new CreateIndexModel<ProductDocument>(
        //         Builders<ProductDocument>.IndexKeys.Ascending(p => p.Sku),
        //         new CreateIndexOptions { Unique = true }
        //     ),
        //     new CreateIndexModel<ProductDocument>(
        //         Builders<ProductDocument>.IndexKeys
        //             .Ascending(p => p.Classification.Group.CategoryId)
        //     ),
        //     
        //     new CreateIndexModel<ProductDocument>(
        //         Builders<ProductDocument>.IndexKeys
        //             .Ascending(p => p.Classification.Group.Id)
        //     ),
        //     
        //     new CreateIndexModel<ProductDocument>(
        //         Builders<ProductDocument>.IndexKeys
        //             .Ascending(p => p.Classification.Vendor.Id)
        //     ),
        //     
        //     new CreateIndexModel<ProductDocument>(
        //         Builders<ProductDocument>.IndexKeys
        //             .Ascending("prices.price")
        //     ),
        //     
        //     new CreateIndexModel<ProductDocument>(
        //         Builders<ProductDocument>.IndexKeys
        //             .Text(p => p.Sku)
        //             .Text(p => p.Title)
        //             .Text(p => p.Attributes.ShortDescription),
        //         new CreateIndexOptions
        //         {
        //             Name = "ProductTextIndex",
        //             Weights = new MongoDB.Bson.BsonDocument
        //             {
        //                 { "Sku", 10 },
        //                 { "Title", 5 },
        //                 { "Attributes.ShortDescription", 1 }
        //             }
        //         }
        //     ),
        //     
        //     new CreateIndexModel<ProductDocument>(
        //         Builders<ProductDocument>.IndexKeys.Ascending(p => p.Attributes.IsPromo)
        //     ),
        //     
        //     new CreateIndexModel<ProductDocument>(
        //         Builders<ProductDocument>.IndexKeys.Ascending("Tags.Name")
        //     )
        // };
        // await Products.Indexes.CreateManyAsync(productIndexes);
        
        var productIndexes = new List<CreateIndexModel<ProductDocument>>();
        var builder = Builders<ProductDocument>.IndexKeys;
        productIndexes.Add(new CreateIndexModel<ProductDocument>(builder.Ascending(p => p.ProductId), new CreateIndexOptions { Unique = true }));
        productIndexes.Add(new CreateIndexModel<ProductDocument>(builder.Ascending(p => p.Sku), new CreateIndexOptions { Unique = true }));
        productIndexes.Add(new CreateIndexModel<ProductDocument>(
            builder.Text(p => p.Sku).Text(p => p.Title).Text(p => p.Attributes.ShortDescription),
            new CreateIndexOptions { Name = "ProductTextIndex", Weights = new MongoDB.Bson.BsonDocument { { "Sku", 10 }, { "Title", 5 }, { "Attributes.ShortDescription", 1 } } }
        ));
        
        var equalityFields = new[] 
        { 
            "Classification.Group.CategoryId", 
            "Classification.Group.Id", 
            "Classification.Vendor.Id",
            "Attributes.IsPromo"
        };
        
        foreach (var field in equalityFields)
        {
            productIndexes.Add(new CreateIndexModel<ProductDocument>(builder.Ascending(field)));
            
            var compositeWithPrice = builder.Combine(
                builder.Ascending(field),
                builder.Ascending("Prices.Price")
            );
            productIndexes.Add(new CreateIndexModel<ProductDocument>(
                compositeWithPrice, 
                new CreateIndexOptions { Name = $"Idx_{field.Replace(".", "")}_Price", Background = true }
            ));
            
            if (field != "Attributes.IsPromo") 
            {
                var compositeComplex = builder.Combine(
                    builder.Ascending(field),
                    builder.Ascending("Attributes.IsPromo"),
                    builder.Ascending("Prices.Price")
                );
                productIndexes.Add(new CreateIndexModel<ProductDocument>(
                    compositeComplex, 
                    new CreateIndexOptions { Name = $"Idx_{field.Replace(".", "")}_Promo_Price", Background = true }
                ));
            }
        }
        productIndexes.Add(new CreateIndexModel<ProductDocument>(builder.Ascending("Tags.Name")));
        await Products.Indexes.CreateManyAsync(productIndexes);

        // Categories
        var categoryIndexes = new List<CreateIndexModel<CategoryDocument>>
        {
            new CreateIndexModel<CategoryDocument>(
                Builders<CategoryDocument>.IndexKeys.Ascending(c => c.Id),
                new CreateIndexOptions { Unique = true }
            ),
            new CreateIndexModel<CategoryDocument>(
                Builders<CategoryDocument>.IndexKeys.Ascending(c => c.Name),
                new CreateIndexOptions { Unique = true }
            ),
            new CreateIndexModel<CategoryDocument>(
                Builders<CategoryDocument>.IndexKeys.Ascending("groups.Id"),
                new CreateIndexOptions 
                { 
                    Unique = true,
                    Sparse = true 
                }
            )
        };
        await Categories.Indexes.CreateManyAsync(categoryIndexes);

        // Vendors
        var vendorIndexes = new List<CreateIndexModel<VendorDocument>>
        {
            new CreateIndexModel<VendorDocument>(
                Builders<VendorDocument>.IndexKeys.Ascending(v => v.Id),
                new CreateIndexOptions { Unique = true }
            )
        };
        await Vendors.Indexes.CreateManyAsync(vendorIndexes);
    
        // ProductTypes
        var productTypeIndexes = new List<CreateIndexModel<ProductTypeDocument>>
        {
            new CreateIndexModel<ProductTypeDocument>(
                Builders<ProductTypeDocument>.IndexKeys.Ascending(pt => pt.Id),
                new CreateIndexOptions { Unique = true }
            )
        };
        await ProductTypes.Indexes.CreateManyAsync(productTypeIndexes);

        // UnitMeasures
        var unitMeasureIndexes = new List<CreateIndexModel<UnitMeasureDocument>>
        {
            new CreateIndexModel<UnitMeasureDocument>(
                Builders<UnitMeasureDocument>.IndexKeys.Ascending(u => u.Id),
                new CreateIndexOptions { Unique = true }
            )
        };
        await UnitMeasures.Indexes.CreateManyAsync(unitMeasureIndexes);

        // Currencies
        var currencyIndexes = new List<CreateIndexModel<CurrencyDocument>>
        {
            new CreateIndexModel<CurrencyDocument>(
                Builders<CurrencyDocument>.IndexKeys.Ascending(c => c.Id),
                new CreateIndexOptions { Unique = true }
            )
        };
        await Currencies.Indexes.CreateManyAsync(currencyIndexes);

        // Tags
        var tagIndexes = new List<CreateIndexModel<TagDocument>>
        {
            new CreateIndexModel<TagDocument>(
                Builders<TagDocument>.IndexKeys.Ascending(t => t.Id),
                new CreateIndexOptions { Unique = true }
            )
        };
        await Tags.Indexes.CreateManyAsync(tagIndexes);
    }
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string CollectionName { get; set; } = "Products";
}

