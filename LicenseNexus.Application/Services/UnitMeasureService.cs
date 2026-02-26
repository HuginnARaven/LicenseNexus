using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class UnitMeasureService(IUnitMeasureRepository unitMeasureRepository, IEventPublisher eventPublisher) : IUnitMeasureService
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

    public async Task<UnitMeasureResponseDto?> AddUnitMeasure(UnitMeasureRequestDto unitMeasureDto)
    {
        var unitMeasure = new UnitMeasure
        {
            Name = unitMeasureDto.Name,
        };

        var result = await unitMeasureRepository.AddAsync(unitMeasure);
        return result == null ? null : new UnitMeasureResponseDto
        {
            Id = result.Id,
            Name = result.Name
        };
    }
    
    public async Task UpdateUnitMeasure(int id, UnitMeasureRequestDto unitMeasureDto)
    {
        var unitMeasure = new UnitMeasure
        {
            Id = id,
            Name = unitMeasureDto.Name,
        };

        await unitMeasureRepository.UpdateAsync(unitMeasure);
        await eventPublisher.PublishAsync(new UnitMeasureUpdatedEvent(unitMeasure));
    }

    public async Task DeleteUnitMeasure(int id)
    {
        await unitMeasureRepository.DeleteAsync(id);
    }
}