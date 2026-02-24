using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Domain.Entities;

[Table("Product_price")]
public class ProductPrice : IEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("term_duration")]
    public string? TermDuration { get; set; }

    [Column("billing_plan")]
    public string? BillingPlan { get; set; }

    [Column("country_code")]
    [StringLength(3)]
    public string? CountryCode { get; set; }

    [Column("segment")]
    public string? Segment { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }
}