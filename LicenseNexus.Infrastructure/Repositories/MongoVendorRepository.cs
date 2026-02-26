using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Exceptions;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoVendorRepository: IVendorRepository
{
    private readonly IMongoCollection<VendorDocument> _collection;
    private readonly MongoContext _context;
    
    public MongoVendorRepository(MongoContext context)
    {
        _collection = context.Vendors;
        _context = context;
    }
    
    public async Task<IEnumerable<Vendor>> GetAllAsync()
    {
        var docs = await _context.Vendors.Find(_ => true).ToListAsync();
        return docs.Select(doc => new Vendor 
        { 
            Id = doc.Id, 
            Name = doc.Name,
            OriginalName = doc.OriginalName,
            Description = doc.Description,
            CountryCode = doc.CountryCode,
            Logo = doc.Logo ?? ""
        });
    }

    public async Task<Vendor?> GetByIdAsync(int id)
    {
        var doc = await _context.Vendors.Find(v => v.Id == id).FirstOrDefaultAsync();
        if (doc == null) return null;
            
        return new Vendor 
        { 
            Id = doc.Id, 
            Name = doc.Name,
            OriginalName = doc.OriginalName,
            Description = doc.Description,
            CountryCode = doc.CountryCode,
            Logo = doc.Logo ?? ""
        };
    }
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<VendorDocument>.Filter.Eq(d => d.Id, id);
        return await _context.Vendors.Find(filter).AnyAsync(cancellationToken);
    }
    
    public async Task<Vendor?> AddAsync(Vendor vendor)
    {
        var id = await _context.GetNextSequenceValueAsync("vendor_id");
        vendor.Id = id;

        var doc = new VendorDocument
        {
            Id = id,
            Name = vendor.Name,
            OriginalName = vendor.OriginalName,
            Description = vendor.Description,
            CountryCode = vendor.CountryCode,
            Logo = vendor.Logo ?? ""
        };

        await _context.Vendors.InsertOneAsync(doc);
        return vendor;
    }

    public async Task UpdateAsync(Vendor vendor)
    {
        var oldDoc = await _context.Vendors.Find(x => x.Id == vendor.Id).FirstOrDefaultAsync();
        var newDoc = new VendorDocument
        {
            Id = vendor.Id,
            Name = vendor.Name,
            OriginalName = vendor.OriginalName,
            Description = vendor.Description,
            CountryCode = vendor.CountryCode,
        };
        newDoc.InternalId = oldDoc.InternalId;
        await _collection.ReplaceOneAsync(x => x.Id == vendor.Id, newDoc);
        
    }

    public async Task DeleteAsync(int id)
    {
        var filter = Builders<ProductDocument>.Filter.Eq(p => p.Classification.Vendor.Id, id);
        bool hasLinkedProducts = await _context.Products.Find(filter).AnyAsync();
        
        if (hasLinkedProducts)
            throw new ConflictException("A database constraint violation occurred. Cannot delete this object because it is assigned to one or more products.");
        
        var deleteFilter = Builders<VendorDocument>.Filter.Eq(v => v.Id, id);
        await _context.Vendors.DeleteOneAsync(deleteFilter);
    }
}