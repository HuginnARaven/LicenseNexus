using FluentValidation;
using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class CurrencyService(
    ICurrencyRepository currencyRepository, 
    IEventPublisher eventPublisher,
    IValidator<CurrencyRequestDto> validator
    ) : ICurrencyService
{
    public async Task<IEnumerable<CurrencyResponseDto>> GetAllCurrencies()
    {
        var categories = await currencyRepository.GetAllAsync();
        return categories.Select(um => new CurrencyResponseDto
        {
            Id = um.Id,
            LiteralCode = um.LiteralCode,
            Name = um.Name,
            CountryCode = um.CountryCode
        });
    }

    public async Task<CurrencyResponseDto?> GetCurrencyById(int id)
    {
        var currency = await currencyRepository.GetByIdAsync(id);
        if (currency == null)
        {
            return null;
        }

        return new CurrencyResponseDto
        {
            Id = currency.Id,
            LiteralCode = currency.LiteralCode,
            Name = currency.Name,
            CountryCode = currency.CountryCode
        };
    }

    public async Task<CurrencyResponseDto?> AddCurrency(CurrencyRequestDto currencyDto)
    {
        await validator.ValidateAndThrowAsync(currencyDto);
        
        var currency = new Currency
        {
            LiteralCode = currencyDto.LiteralCode,
            Name = currencyDto.Name,
            CountryCode = currencyDto.CountryCode
        };

        var result = await currencyRepository.AddAsync(currency);
        return result == null ? null : new CurrencyResponseDto
        {
            Id = result.Id,
            LiteralCode = result.LiteralCode,
            Name = result.Name,
            CountryCode = result.CountryCode
        };
    }

    public async Task UpdateCurrency(int id, CurrencyRequestDto currencyDto)
    {
        await validator.ValidateAndThrowAsync(currencyDto);
        
        var currency = new Currency
        {
            Id = id,
            LiteralCode = currencyDto.LiteralCode,
            Name = currencyDto.Name,
            CountryCode = currencyDto.CountryCode
        };

        await currencyRepository.UpdateAsync(currency);
        await eventPublisher.PublishAsync(new CurrencyUpdatedEvent(currency));
    }

    public async Task DeleteCurrency(int id)
    {
        await currencyRepository.DeleteAsync(id);
    }
}