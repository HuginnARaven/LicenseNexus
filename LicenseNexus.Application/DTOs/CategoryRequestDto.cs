namespace LicenseNexus.Application.DTOs;

public class CategoryRequestDto
{
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
}