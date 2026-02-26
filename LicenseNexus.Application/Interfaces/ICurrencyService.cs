using LicenseNexus.Application.DTOs;

namespace LicenseNexus.Application.Interfaces;

public interface ICurrencyService
{
    Task<IEnumerable<CurrencyResponseDto>> GetAllCurrencies();
    Task<CurrencyResponseDto?> GetCurrencyById(int id);
    Task<CurrencyResponseDto?> AddCurrency(CurrencyRequestDto currencyDto);
    Task UpdateCurrency(int id, CurrencyRequestDto currencyDto);
    Task DeleteCurrency(int id);
}