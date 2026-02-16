using LicenseNexus.Infrastructure.Data.MongoEntities;
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
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string CollectionName { get; set; } = "Products";
}

