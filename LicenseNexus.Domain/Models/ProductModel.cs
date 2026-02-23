namespace LicenseNexus.Domain.Models;

public class ProductModel
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<TagModel> Tags { get; set; } = new();
    public ClassificationModel Classification { get; set; } = new();
    public AttributesModel Attributes { get; set; } = new();
    public List<DescriptionModel> Descriptions { get; set; } = new();
    public CurrencyModel Currency { get; set; } = new();
    public List<ProductPriceModel> Prices { get; set; } = new();
}

public class TagModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ClassificationModel
{
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int UnitMeasureId { get; set; }
    public string UnitMeasureName { get; set; } = string.Empty;
        
    public VendorModel Vendor { get; set; } = new();
    public GroupModel Group { get; set; } = new();
}

public class VendorModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}

public class GroupModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class AttributesModel
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
    public DateTime CreatedDate { get; set; }
    public string? Author { get; set; }
}

public class DescriptionModel
{
    public int Id { get; set; }
    public string FullText { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
}

public class ProductPriceModel
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public int? TermDuration { get; set; }
    public string? BillingPlan { get; set; }
    public string? Segment { get; set; }
    public string? CountryCode { get; set; }
    public DateTime? StartDate { get; set; }
}

public class CurrencyModel
{
    public int Id { get; set; }
    public string LiteralCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}