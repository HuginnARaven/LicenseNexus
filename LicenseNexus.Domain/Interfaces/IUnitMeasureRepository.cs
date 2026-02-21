using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IUnitMeasureRepository
{
    Task<IEnumerable<UnitMeasure>> GetAllAsync();
    Task<UnitMeasure?> GetByIdAsync(int id);
    Task AddAsync(UnitMeasure unitMeasure);
    Task UpdateAsync(UnitMeasure unitMeasure);
    //TODO: mb add Update/Delete 
}