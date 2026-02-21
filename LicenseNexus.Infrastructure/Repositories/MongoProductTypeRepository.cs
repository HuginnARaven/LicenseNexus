using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoProductTypeRepository: IProductTypeRepository
{
    private readonly MongoContext _context;
    
    public MongoProductTypeRepository(MongoContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<ProductType>> GetAllAsync()
    {
        var docs = await _context.ProductTypes.Find(_ => true).ToListAsync();
        return docs.Select(doc => new ProductType 
        { 
            Id = doc.Id, 
            TypeName = doc.TypeName
        });
    }

    public async Task<ProductType?> GetByIdAsync(int id)
    {
        var doc = await _context.ProductTypes.Find(pt => pt.Id == id).FirstOrDefaultAsync();
        if (doc == null) return null;
            
        return new ProductType 
        { 
            Id = doc.Id, 
            TypeName = doc.TypeName
        };
    }

    public async Task AddAsync(ProductType productType)
    {
        var id = await _context.GetNextSequenceValueAsync("product_type_id");
        productType.Id = id;

        var doc = new ProductTypeDocument
        {
            Id = id,
            TypeName = productType.TypeName
        };

        await _context.ProductTypes.InsertOneAsync(doc);
    }

    public async Task UpdateAsync(ProductType productType)
    {
        var filter = Builders<ProductTypeDocument>.Filter.Eq(pt => pt.Id, productType.Id);
        var update = Builders<ProductTypeDocument>.Update
            .Set(pt => pt.TypeName, productType.TypeName);

        await _context.ProductTypes.UpdateOneAsync(filter, update);
    }
}