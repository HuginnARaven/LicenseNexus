using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseNexus.Domain.Entities;

[Table("Tag")]
public class Tag
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [Required]
    public string Name { get; set; } = string.Empty;
    
    // Navigation Properties
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}