namespace LicenseNexus.Domain.Models;

public class ProductPatchFieldsModel
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
    
    public int? UnitMeasureId { get; set; }
    public string? UnitMeasureName { get; set; }
    
    public int? ProductTypeId { get; set; }
    public string? ProductTypeName { get; set; }
    
    public VendorModel? Vendor { get; set; }
    public GroupModel? Group { get; set; }
    public CurrencyModel? Currency { get; set; }
}