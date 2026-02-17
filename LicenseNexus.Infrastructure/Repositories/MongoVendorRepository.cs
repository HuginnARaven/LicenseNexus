using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoVendorRepository: IVendorRepository
{
    private readonly MongoContext _context;
    
    public MongoVendorRepository(MongoContext context)
    {
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

    public async Task AddAsync(Vendor vendor)
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
    }
}