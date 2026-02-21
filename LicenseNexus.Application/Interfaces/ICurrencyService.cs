using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface ICurrencyService
{
    Task<IEnumerable<CurrencyResponseDto>> GetAllCurrencies();
    Task<CurrencyResponseDto?> GetCurrencyById(int id);
    Task AddCurrency(CurrencyRequestDto currencyDto);
    Task UpdateCurrency(int id, CurrencyRequestDto currencyDto);
}