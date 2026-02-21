using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoUnitMeasureRepository: IUnitMeasureRepository
{
    private readonly MongoContext _context;
    
    public MongoUnitMeasureRepository(MongoContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<UnitMeasure>> GetAllAsync()
    {
        var docs = await _context.UnitMeasures.Find(_ => true).ToListAsync();
        return docs.Select(doc => new UnitMeasure 
        { 
            Id = doc.Id, 
            Name = doc.Name
        });
    }

    public async Task<UnitMeasure?> GetByIdAsync(int id)
    {
        var doc = await _context.UnitMeasures.Find(um => um.Id == id).FirstOrDefaultAsync();
        if (doc == null) return null;
            
        return new UnitMeasure 
        { 
            Id = doc.Id, 
            Name = doc.Name
        };
    }

    public async Task AddAsync(UnitMeasure unitMeasure)
    {
        var id = await _context.GetNextSequenceValueAsync("unit_measure_id");
        unitMeasure.Id = id;

        var doc = new UnitMeasureDocument
        {
            Id = id,
            Name = unitMeasure.Name
        };

        await _context.UnitMeasures.InsertOneAsync(doc);
    }

    public async Task UpdateAsync(UnitMeasure unitMeasure)
    {
        var oldDoc = await _context.UnitMeasures.Find(um => um.Id == unitMeasure.Id).FirstOrDefaultAsync();
        var newDoc = new UnitMeasureDocument
        {
            Id = unitMeasure.Id,
            Name = unitMeasure.Name
        };
        newDoc.InternalId = oldDoc.InternalId;
        await _context.UnitMeasures.ReplaceOneAsync(um => um.Id == unitMeasure.Id, newDoc);
    }
}