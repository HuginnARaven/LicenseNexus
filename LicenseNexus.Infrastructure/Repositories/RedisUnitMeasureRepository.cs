using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisUnitMeasureRepository: IUnitMeasureRepository
{
    private ExtendedSqlContext _context;
    
    public RedisUnitMeasureRepository(ExtendedSqlContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<UnitMeasure>> GetAllAsync()
    {
        return await _context.UnitMeasures.Where(_ => true).ToListAsync();
    }

    public async Task<UnitMeasure?> GetByIdAsync(int id)
    {
        return await _context.UnitMeasures.FirstOrDefaultAsync(um => um.Id == id);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.UnitMeasures.AnyAsync(um => um.Id == id, cancellationToken);
    }
    
    public async Task<UnitMeasure?> AddAsync(UnitMeasure unitMeasure)
    {
        _context.UnitMeasures.Add(unitMeasure);
        var res = await _context.SaveChangesAsync();
        if (res > 0)
            return unitMeasure;
        
        return null;
    }

    public async Task UpdateAsync(UnitMeasure unitMeasure)
    {
        _context.UnitMeasures.Update(unitMeasure);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var unitMeasure = await _context.UnitMeasures.FindAsync(id);
        if (unitMeasure == null)
            throw new InvalidOperationException("Object Not Found");
        _context.Remove(unitMeasure);
        await  _context.SaveChangesAsync();
    }
}