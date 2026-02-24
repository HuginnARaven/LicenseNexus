using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseNexus.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Domain.Entities;

[Table("Product_group")]
[Index(nameof(CategoryId), nameof(Name), IsUnique = true)]
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
    public string Author { get; set; } = string.Empty;

    [Column("category_id")]
    public int CategoryId { get; set; }

    // Navigation Properties
    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }
}