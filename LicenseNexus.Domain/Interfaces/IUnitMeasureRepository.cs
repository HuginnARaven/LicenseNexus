using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Domain.Interfaces;

public interface IUnitMeasureRepository
{
    Task<IEnumerable<UnitMeasure>> GetAllAsync();
    Task<UnitMeasure?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<UnitMeasure?> AddAsync(UnitMeasure unitMeasure);
    Task UpdateAsync(UnitMeasure unitMeasure);
    Task DeleteAsync(int id);
}