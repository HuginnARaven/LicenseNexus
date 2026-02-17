namespace LicenseNexus.Application.DTOs;

public class CategoryResponseDTO
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Author { get; set; }
}