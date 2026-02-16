namespace LicenseNexus.Infrastructure.Data.RedisEntities;

public class DescriptionModel
{
    public string Description { get; set; } = string.Empty; // Short description
    public string FullText { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
}

public class ProductPriceModel
{
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public int? TermDuration { get; set; }
    public string? BillingPlan { get; set; }
    public string? Segment { get; set; }
    public string? CountryCode { get; set; }
    public DateTime? StartDate { get; set; }
}