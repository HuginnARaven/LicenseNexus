namespace LicenseNexus.Application.DTOs;

public class CurrencyRequestDto
{
    public string LiteralCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}