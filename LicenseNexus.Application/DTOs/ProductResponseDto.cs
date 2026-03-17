namespace LicenseNexus.Application.DTOs;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Tags { get; set; } = new();
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int UnitMeasureId { get; set; }
    public string UnitMeasureName { get; set; } = string.Empty;
    public ProductVendorDto Vendor { get; set; } = new();
    public ProductGroupDto Group { get; set; } = new();
    public ProductAttributesDto Attributes { get; set; } = new();
    public List<ProductDescriptionDto> Descriptions { get; set; } = new();
    public ProductPriceDto Currency { get; set; } = new();
    public List<ProductCurrencyDto> Prices { get; set; } = new();
}

public class ProductVendorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CountryCode { get; set; } = string.Empty;
}

public class ProductGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductAttributesDto
{
    public string ShortDescription { get; set; } = string.Empty;
    public int QuantityMin { get; set; }
    public int QuantityMax { get; set; }
    public bool IsPromo { get; set; }
    public bool IsTop { get; set; }
    public bool IsNew { get; set; }
    public string? Logo { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ProductDescriptionDto
{
    public int Id { get; set; }
    public string FullText { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
}

public class ProductPriceDto
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public string? TermDuration { get; set; }
    public string? BillingPlan { get; set; }
    public string? Segment { get; set; }
    public string? CountryCode { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
}

public class ProductCurrencyDto
{
    public int Id { get; set; }
    public string LiteralCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}