using System.Text.Json;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.RedisEntities;
using LicenseNexus.Infrastructure.Services;
using StackExchange.Redis;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisProductRepository: IProductRepository
{
    private readonly ExtendedSqlContext _sqlContext;
    private readonly IDatabase _redisDb;
    private readonly IProductAggregatorService _aggregator;

    public RedisProductRepository(ExtendedSqlContext sqlContext, RedisContext redisContext, IProductAggregatorService aggregator)
    {
        _sqlContext = sqlContext;
        _redisDb = redisContext.Database;
        _aggregator = aggregator;
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
    
    private Product MapRedisToDomain(ProductModel model)
    {
        throw new NotImplementedException();
    }
}