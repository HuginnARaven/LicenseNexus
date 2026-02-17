namespace LicenseNexus.Application.DTOs;

public class VendorResponceDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OriginalName { get; set; }
    public string? Description { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string? Logo { get; set; }
}