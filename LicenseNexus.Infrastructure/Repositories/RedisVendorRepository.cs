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
    
    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Vendors.AnyAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Vendor?> AddAsync(Vendor vendor)
    {
        _context.Vendors.Add(vendor);
        var res = await _context.SaveChangesAsync();
        if (res > 0)
            return vendor;
        
        return null;
    }

    public async Task UpdateAsync(Vendor vendor)
    {
        var oldVendor = await _context.Vendors.FindAsync(vendor.Id);
        if (oldVendor != null)
        {
            oldVendor.Name = vendor.Name;
            oldVendor.OriginalName = vendor.OriginalName;
            oldVendor.Description = vendor.Description;
            oldVendor.CountryCode = vendor.CountryCode;
            await  _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        // if (await _context.Vendors.Where(v => v.Id == id).ExecuteDeleteAsync() <=0)
        //     throw new InvalidOperationException("Object Not Found");
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            throw new InvalidOperationException("Object Not Found");
        _context.Remove(vendor);
        await  _context.SaveChangesAsync();
    }
}