using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.Services;

public class CurrencyService(ICurrencyRepository currencyRepository) : ICurrencyService
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

    public async Task AddCurrency(CurrencyRequestDto currencyDto)
    {
        var currency = new Currency
        {
            LiteralCode = currencyDto.LiteralCode,
            Name = currencyDto.Name,
            CountryCode = currencyDto.CountryCode
        };

        await currencyRepository.AddAsync(currency);
    }
}