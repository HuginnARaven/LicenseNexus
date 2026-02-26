using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisCategoryRepository: ICategoryRepository
{
    private readonly ExtendedSqlContext _context;

    public RedisCategoryRepository(ExtendedSqlContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.Include(c => c.ProductGroups).Where(_ => true).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.Include(c => c.ProductGroups).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> AddAsync(Category category)
    {
        _context.Categories.Add(category);
        var res = await _context.SaveChangesAsync();
        if (res > 0)
            return category;
        
        return null;
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = new Category { Id = id };
        _context.Categories.Attach(category);
        _context.Categories.Remove(category);
  
        await _context.SaveChangesAsync();
    }
}