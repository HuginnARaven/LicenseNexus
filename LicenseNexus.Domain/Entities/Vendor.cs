using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseNexus.Domain.Entities;

[Table("Vendor")]
public class Vendor
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Column("original_name")]
    public string? OriginalName { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("country_code")]
    [StringLength(3)]
    public string CountryCode { get; set; } = string.Empty;

    [Column("logo")]
    public string? Logo { get; set; }
}