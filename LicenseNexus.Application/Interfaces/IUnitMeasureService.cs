using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface IUnitMeasureService
{
    Task<IEnumerable<UnitMeasureResponseDto>> GetAllUnitMeasures();
    Task<UnitMeasureResponseDto?> GetUnitMeasureById(int id);
    Task<UnitMeasureResponseDto?> AddUnitMeasure(UnitMeasureRequestDto unitMeasureDto);
    Task UpdateUnitMeasure(int id, UnitMeasureRequestDto unitMeasureDto);
    Task DeleteUnitMeasure(int id);
}