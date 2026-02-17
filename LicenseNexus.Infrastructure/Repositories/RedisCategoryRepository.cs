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
        return await _context.Categories.Where(_ => true).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }
}