using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoEntities;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoProductRepository: IProductRepository
{
    private readonly IMongoCollection<ProductDocument> _collection;
    
    public MongoProductRepository(MongoContext context)
    {
        _collection = context.Products;
    }
    
    public async Task<Product?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task AddAsync(Product product)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Product product)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}