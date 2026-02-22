using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Domain.Entities;

[Table("Tag")]
public class Tag : IEntity
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