using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface ICurrencyRepository
{
    Task<IEnumerable<Currency>> GetAllAsync();
    Task<Currency?> GetByIdAsync(int id);
    Task AddAsync(Currency currency);
    //TODO: mb add Update/Delete 
}