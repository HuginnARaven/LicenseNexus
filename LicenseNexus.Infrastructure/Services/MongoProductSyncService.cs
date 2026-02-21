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

    public async Task UpdateCategoryAsync(Category category, CancellationToken ct)
    {
        var filter = Builders<ProductDocument>.Filter.Eq(p => p.Classification.Group.CategoryId, category.Id);
        var update = Builders<ProductDocument>.Update.Set(p => p.Classification.Group.CategoryName, category.CategoryName);
        await _collection.UpdateManyAsync(filter, update, cancellationToken: ct);
    }

    public Task UpdateGroupAsync(ProductGroup group, CancellationToken ct)
    {
        var filter = Builders<ProductDocument>.Filter.Eq(p => p.Classification.Group.Id, group.Id);
        var update = Builders<ProductDocument>.Update.Set(p => p.Classification.Group.Name, group.Name);
        return _collection.UpdateManyAsync(filter, update, cancellationToken: ct);
    }

    public async Task UpdateProductTypeAsync(ProductType productType, CancellationToken ct)
    {
        var filter = Builders<ProductDocument>.Filter.Eq(p => p.Classification.TypeId, productType.Id);
        var update = Builders<ProductDocument>.Update.Set(p => p.Classification.TypeName, productType.TypeName);
        await _collection.UpdateManyAsync(filter, update, cancellationToken: ct);
    }

    public async Task UpdateUnitMeasureAsync(UnitMeasure unitMeasure, CancellationToken ct)
    {
        var filter = Builders<ProductDocument>.Filter.Eq(p => p.Classification.UnitMeasureId, unitMeasure.Id);
        var update = Builders<ProductDocument>.Update.Set(p => p.Classification.UnitMeasureName, unitMeasure.Name);
        await _collection.UpdateManyAsync(filter, update, cancellationToken: ct);
    }

    public async Task UpdateCurrencyAsync(Currency currency, CancellationToken ct)
    {
        var filter = Builders<ProductDocument>.Filter.Eq(p => p.Currency.Id, currency.Id);

        var newCurrencyDoc = new CurrencyDoc
        {
            Id = currency.Id,
            LiteralCode = currency.LiteralCode,
            Name = currency.Name
        };
        
        var update = Builders<ProductDocument>.Update.Set(p => p.Currency, newCurrencyDoc);
        await _collection.UpdateManyAsync(filter, update, cancellationToken: ct);
    }
}