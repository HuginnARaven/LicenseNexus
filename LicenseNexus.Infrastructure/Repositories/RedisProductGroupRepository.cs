using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisProductGroupRepository: IProductGroupRepository
{
    private readonly ExtendedSqlContext _context;
    
    public RedisProductGroupRepository(ExtendedSqlContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<ProductGroup>> GetAllAsync()
    {
        return await _context.ProductGroups.Where(_ => true).ToListAsync();
    }

    public async Task<ProductGroup?> GetByIdAsync(int id)
    {
        return await _context.ProductGroups.FirstOrDefaultAsync(pg => pg.Id == id);
    }

    public async Task AddAsync(ProductGroup group)
    {
        _context.ProductGroups.Add(group);
        await _context.SaveChangesAsync();
    }
}