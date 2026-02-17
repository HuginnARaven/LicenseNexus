using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class UnitMeasureService(IUnitMeasureRepository unitMeasureRepository) : IUnitMeasureService
{
    public async Task<IEnumerable<UnitMeasureResponseDto>> GetAllUnitMeasures()
    {
        var categories = await unitMeasureRepository.GetAllAsync();
        return categories.Select(um => new UnitMeasureResponseDto
        {
            Id = um.Id,
            Name = um.Name,
        });
    }

    public async Task<UnitMeasureResponseDto?> GetUnitMeasureById(int id)
    {
        var unitMeasure = await unitMeasureRepository.GetByIdAsync(id);
        if (unitMeasure == null)
        {
            return null;
        }

        return new UnitMeasureResponseDto
        {
            Id = unitMeasure.Id,
            Name = unitMeasure.Name,
        };
    }

    public async Task AddUnitMeasure(UnitMeasureRequestDto unitMeasureDto)
    {
        var unitMeasure = new UnitMeasure
        {
            Name = unitMeasureDto.Name,
        };

        await unitMeasureRepository.AddAsync(unitMeasure);
    }
}