using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisProductTypeRepository: IProductTypeRepository
{
    private ExtendedSqlContext _context;
    
    public RedisProductTypeRepository(ExtendedSqlContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<ProductType>> GetAllAsync()
    {
        return await _context.ProductTypes.Where(_ => true).ToListAsync();
    }

    public async Task<ProductType?> GetByIdAsync(int id)
    {
        return await _context.ProductTypes.FirstOrDefaultAsync(pt => pt.Id == id);
    }
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductTypes.AnyAsync(pt => pt.Id == id, cancellationToken);
    }

    public async Task AddAsync(ProductType productType)
    {
        _context.ProductTypes.Add(productType);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProductType productType)
    {
        _context.ProductTypes.Update(productType);
        await _context.SaveChangesAsync();
    }
}