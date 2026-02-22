namespace LicenseNexus.Application.DTOs;

public class CustomerRequestDto
{
    public int PartnerId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? ZipCode { get; set; }
    public string CountryCode { get; set; } = string.Empty;
}