using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseNexus.Domain.Entities;

[Table("Product")]
public class Product
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sku")]
    public string? Sku { get; set; }

    [Column("title")]
    [Required]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? ShortDescription { get; set; }
        
    [Column("vendor_id")]
    public int VendorId { get; set; }

    [Column("product_type_id")]
    public int ProductTypeId { get; set; }

    [Column("unit_measure_id")]
    public int UnitMeasureId { get; set; }

    [Column("currency_id")]
    public int CurrencyId { get; set; }

    [Column("product_group_id")]
    public int ProductGroupId { get; set; }
        
    [Column("quantity_min")]
    public int QuantityMin { get; set; }

    [Column("quantity_max")]
    public int QuantityMax { get; set; }

    [Column("start_date")]
    public DateTime? StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }
        
    [Column("is_promo")]
    public bool IsPromo { get; set; }

    [Column("is_top")]
    public bool IsTop { get; set; }

    [Column("is_new")]
    public bool IsNew { get; set; }

    [Column("logo")]
    public string? Logo { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("autor")]
    public string? Author { get; set; }


    // Navigation Properties
    [ForeignKey("VendorId")]
    public Vendor? Vendor { get; set; }

    [ForeignKey("ProductTypeId")]
    public ProductType? ProductType { get; set; }

    [ForeignKey("UnitMeasureId")]
    public UnitMeasure? UnitMeasure { get; set; }

    [ForeignKey("CurrencyId")]
    public Currency? Currency { get; set; }

    [ForeignKey("ProductGroupId")]
    public ProductGroup? ProductGroup { get; set; }

    // One-to-Many relations
    public ICollection<ProductPrice> Prices { get; set; } = new List<ProductPrice>();
    public ICollection<FullDescription> FullDescriptions { get; set; } = new List<FullDescription>();
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}