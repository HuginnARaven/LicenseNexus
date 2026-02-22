namespace LicenseNexus.Application.DTOs;

public class ProductPriceRequestDto
{
    public decimal Price { get; set; }
    public int? TermDuration { get; set; }
    public string? BillingPlan { get; set; }
    public string? CountryCode { get; set; }
    public string? Segment { get; set; }
    public DateTime? StartDate { get; set; }
}