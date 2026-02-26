using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Exceptions;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoCurrencyRepository: ICurrencyRepository
{
    private readonly MongoContext _context;
    
    public MongoCurrencyRepository(MongoContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Currency>> GetAllAsync()
    {
        var docs = await _context.Currencies.Find(_ => true).ToListAsync();
        return docs.Select(doc => new Currency 
        { 
            Id = doc.Id, 
            LiteralCode = doc.LiteralCode,
            Name = doc.Name,
            CountryCode = doc.CountryCode
        });
    }

    public async Task<Currency?> GetByIdAsync(int id)
    {
        var doc = await _context.Currencies.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (doc == null) return null;
            
        return new Currency 
        { 
            Id = doc.Id, 
            LiteralCode = doc.LiteralCode,
            Name = doc.Name,
            CountryCode = doc.CountryCode
        };
    }
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<CurrencyDocument>.Filter.Eq(v => v.Id, id);
        return await _context.Currencies.Find(filter).AnyAsync(cancellationToken);
    }

    public async Task<Currency?> AddAsync(Currency currency)
    {
        var id = await _context.GetNextSequenceValueAsync("currency_id");
        currency.Id = id;

        var doc = new CurrencyDocument
        {
            Id = id,
            LiteralCode = currency.LiteralCode,
            Name = currency.Name,
            CountryCode = currency.CountryCode
        };

        await _context.Currencies.InsertOneAsync(doc);
        return currency;
    }

    public async Task UpdateAsync(Currency currency)
    {
        var filter = Builders<CurrencyDocument>.Filter.Eq(c => c.Id, currency.Id);
        var update = Builders<CurrencyDocument>.Update
            .Set(c => c.LiteralCode, currency.LiteralCode)
            .Set(c => c.Name, currency.Name)
            .Set(c => c.CountryCode, currency.CountryCode);

        await _context.Currencies.UpdateOneAsync(filter, update);
    }

    public async Task DeleteAsync(int id)
    {
        var filter = Builders<ProductDocument>.Filter.Eq(p => p.Currency.Id, id);
        bool hasLinkedProducts = await _context.Products.Find(filter).AnyAsync();
        
        if (hasLinkedProducts)
            throw new ConflictException("A database constraint violation occurred. Cannot delete this object because it is assigned to one or more products.");
        
        var deleteFilter = Builders<CurrencyDocument>.Filter.Eq(v => v.Id, id);
        await _context.Currencies.DeleteOneAsync(deleteFilter);
    }
}