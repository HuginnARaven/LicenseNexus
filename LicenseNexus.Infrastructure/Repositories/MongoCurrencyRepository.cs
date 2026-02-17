using LicenseNexus.Domain.Entities;
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

    public async Task AddAsync(Currency currency)
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
    }
}