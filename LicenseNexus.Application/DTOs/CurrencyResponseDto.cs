namespace LicenseNexus.Application.DTOs;

public class CurrencyResponseDto
{
    public int Id { get; set; }
    public string LiteralCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}