using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseNexus.Domain.Entities;

[Table("Category")]
public class Category
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("active_is")]
    public bool IsActive { get; set; }

    [Column("category_name")]
    [Required]
    public string CategoryName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("autor")]
    public string? Author { get; set; }

    // Navigation Properties
    public ICollection<ProductGroup> ProductGroups { get; set; } = new List<ProductGroup>();
}