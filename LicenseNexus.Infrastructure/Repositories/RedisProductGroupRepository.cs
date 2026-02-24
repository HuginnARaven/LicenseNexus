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

    public async Task<ProductGroup?>  AddAsync(ProductGroup group)
    {
        _context.ProductGroups.Add(group);
        var res = await _context.SaveChangesAsync();
        if (res > 0)
            return group;
        
        return null;
    }

    public async Task UpdateAsync(ProductGroup group)
    {
        await _context.ProductGroups.Where(g => g.Id == group.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(g => g.Name, group.Name)
            .SetProperty(g => g.IsActive, group.IsActive)
            .SetProperty(g => g.Note, group.Note)
            .SetProperty(g => g.Author, group.Author)
        );
    }
}