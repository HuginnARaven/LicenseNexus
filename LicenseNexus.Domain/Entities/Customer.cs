using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseNexus.Domain.Entities;

[Table("Customer")]
public class Customer
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("partner_id")]
    public int PartnerId { get; set; } // FK

    [Column("account_name")]
    public string AccountName { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("legal_name")]
    public string LegalName { get; set; } = string.Empty;

    [Column("city")]
    public string? City { get; set; }

    [Column("region")]
    public string? Region { get; set; }

    [Column("zip_code")]
    public string? ZipCode { get; set; }

    [Column("country_code")]
    [StringLength(3)]
    public string CountryCode { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation Property
    [ForeignKey("PartnerId")]
    public Partner? Partner { get; set; }
}