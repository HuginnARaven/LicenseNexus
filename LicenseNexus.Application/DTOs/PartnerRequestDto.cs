namespace LicenseNexus.Application.DTOs;

public class PartnerRequestDto
{
    public string CountryCode { get; set; } = string.Empty;
    public string FullCompanyName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Author { get; set; } = string.Empty;
}