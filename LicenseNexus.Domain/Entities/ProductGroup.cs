using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Domain.Entities;

[Table("Product_group")]
public class ProductGroup : IEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Column("active_is")]
    public bool IsActive { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("autor")]
    public string? Author { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    // Navigation Properties
    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }
}