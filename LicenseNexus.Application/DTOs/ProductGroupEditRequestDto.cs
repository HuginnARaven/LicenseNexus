namespace LicenseNexus.Application.DTOs;

public class ProductGroupEditRequestDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public string? Author { get; set; }
}