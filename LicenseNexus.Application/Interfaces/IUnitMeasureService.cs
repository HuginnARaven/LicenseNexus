using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface IUnitMeasureService
{
    Task<IEnumerable<UnitMeasureResponseDto>> GetAllUnitMeasures();
    Task<UnitMeasureResponseDto?> GetUnitMeasureById(int id);
    Task AddUnitMeasure(UnitMeasureRequestDto unitMeasureDto);
}