using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseNexus.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Domain.Entities;

[Table("Category")]
[Index(nameof(CategoryName), IsUnique = true)]
public class Category : IEntity
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
    public string Author { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<ProductGroup> ProductGroups { get; set; } = new List<ProductGroup>();
}