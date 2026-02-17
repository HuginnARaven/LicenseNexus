namespace LicenseNexus.Application.DTOs;

public class ProductGroupResponseDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Author { get; set; }
    public int CategoryId { get; set; }
}