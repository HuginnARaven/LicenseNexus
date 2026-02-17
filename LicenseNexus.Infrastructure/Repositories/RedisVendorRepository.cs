using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Infrastructure.Repositories;

public class RedisVendorRepository: IVendorRepository
{
    private ExtendedSqlContext _context;
    
    public RedisVendorRepository(ExtendedSqlContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Vendor>> GetAllAsync()
    {
        return await _context.Vendors.Where(_ => true).ToListAsync();
    }

    public async Task<Vendor?> GetByIdAsync(int id)
    {
        return await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task AddAsync(Vendor vendor)
    {
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();
    }
}