namespace LicenseNexus.Application.DTOs;

public class ProductGroupRequestDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public string Author { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}