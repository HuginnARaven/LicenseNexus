using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseNexus.Domain.Entities;

[Table("Product_tag")]
public class ProductTag
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("tag_id")]
    public int TagId { get; set; }

    // Navigation Properties
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [ForeignKey("TagId")]
    public Tag? Tag { get; set; }
}