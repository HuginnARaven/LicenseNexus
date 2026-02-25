using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface ICurrencyRepository
{
    Task<IEnumerable<Currency>> GetAllAsync();
    Task<Currency?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task AddAsync(Currency currency);
    Task UpdateAsync(Currency currency);
    //TODO: mb add Delete 
}