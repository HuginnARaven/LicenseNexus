using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Services;

public class MongoProductSyncService: IProductSyncService
{
    private readonly IMongoCollection<ProductDocument> _collection;
    private readonly MongoContext _context;

    public MongoProductSyncService(MongoContext context)
    {
        _collection = context.Products;
        _context = context;
    }
    
    public async Task UpdateVendorAsync(Vendor vendor, CancellationToken ct)
    {
        var filter = Builders<ProductDocument>.Filter.Eq(p => p.Classification.Vendor.Id, vendor.Id);
        var newVendorDoc = new VendorDoc
        {
            Id = vendor.Id,
            Name = vendor.Name,
            CountryCode = vendor.CountryCode
        };
        
        var update = Builders<ProductDocument>.Update.Set(p => p.Classification.Vendor, newVendorDoc);
        await _collection.UpdateManyAsync(filter, update, cancellationToken: ct);
    }
}