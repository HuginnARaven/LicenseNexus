namespace LicenseNexus.Application.DTOs;

public class ProductFilterDto
{
    public int? CategoryId { get; set; }
    public int? GroupId { get; set; }
    public int? VendorId { get; set; }
    public string? Search { get; set; }
    public double? PriceFrom { get; set; }
    public double? PriceTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}