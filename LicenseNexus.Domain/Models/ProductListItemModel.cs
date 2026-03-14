using System.Text.Json.Serialization;

namespace LicenseNexus.Domain.Models;

[JsonConverter(typeof(ProductListItemModelConverter))]
public class ProductListItemModel
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    
    public bool IsPromo { get; set; }
    public bool IsTop { get; set; }
    public bool IsNew { get; set; }
    public string? Logo { get; set; }
    
    public string VendorName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string CurrencyLiteralCode { get; set; } = string.Empty;
}