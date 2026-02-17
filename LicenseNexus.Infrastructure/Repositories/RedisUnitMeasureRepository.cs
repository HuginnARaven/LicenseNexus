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

    public async Task AddAsync(UnitMeasure unitMeasure)
    {
        _context.UnitMeasures.Add(unitMeasure);
        await _context.SaveChangesAsync();
    }
}