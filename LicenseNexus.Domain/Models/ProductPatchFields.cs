namespace LicenseNexus.Domain.Models;

public class ProductPatchFields
{
    public string? Sku { get; set; }
    public string? Title { get; set; }
    public string? ShortDescription { get; set; }
    public int? QuantityMin { get; set; }
    public int? QuantityMax { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsPromo { get; set; }
    public bool? IsTop { get; set; }
    public bool? IsNew { get; set; }
    public string? Logo { get; set; }
    public string? Author { get; set; }
    
    public int? VendorId { get; set; }
    public int? ProductTypeId { get; set; }
    public int? UnitMeasureId { get; set; }
    public int? CurrencyId { get; set; }
    public int? ProductGroupId { get; set; }
}