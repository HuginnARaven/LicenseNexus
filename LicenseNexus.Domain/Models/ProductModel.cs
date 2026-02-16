namespace LicenseNexus.Domain.Models;

public class ProductModel
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Tags { get; set; } = new();
    public Classification Classification { get; set; } = new();
    public Attributes Attributes { get; set; } = new();
    public List<Description> Descriptions { get; set; } = new();
    public int? CurrencyId { get; set; } // Only for updating Product.Currency in mssql
    public string CurrencyCode { get; set; } = string.Empty;
    public List<ProductPrice> Prices { get; set; } = new();
}

public class Classification
{
    public int? TypeId { get; set; } // Only for updating Product.Product_type in mssql
    public string TypeName { get; set; } = string.Empty;
    public int? UnitMeasureId { get; set; } // Only for updating Product.Unit_measure in mssql
    public string UnitMeasureName { get; set; } = string.Empty;
        
    public Vendor Vendor { get; set; } = new();
    public Group Group { get; set; } = new();
}

public class Vendor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class Attributes
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

public class Description
{
    public int? Id { get; set; } // Only for updating Product.Full_description in mssql
    public string FullText { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
}

public class ProductPrice
{
    public int? Id { get; set; } // Only for updating Product.Product_price in mssql
    public decimal Price { get; set; }
    public int? TermDuration { get; set; }
    public string? BillingPlan { get; set; }
    public string? Segment { get; set; }
    public string? CountryCode { get; set; }
    public DateTime? StartDate { get; set; }
}