using System.Collections.Generic;
using System.Threading.Tasks;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IVendorRepository
{
    Task<IEnumerable<Vendor>> GetAllAsync();
    Task<Vendor?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Vendor vendor);
    Task UpdateAsync(Vendor vendor);
    //TODO: mb add Delete 
}
