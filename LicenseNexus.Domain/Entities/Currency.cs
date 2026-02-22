using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Domain.Entities;

[Table("Currency")]
public class Currency : IEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("literal_code")]
    [Required]
    [StringLength(3)]
    public string LiteralCode { get; set; } = string.Empty; // USD, UAH

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("country_code")]
    [StringLength(3)]
    public string CountryCode { get; set; } = string.Empty;
}