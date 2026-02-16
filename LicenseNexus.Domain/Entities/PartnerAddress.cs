using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseNexus.Domain.Entities;


[Table("Partner_address")]
public class PartnerAddress
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("partner_id")]
    public int PartnerId { get; set; } // FK

    [Column("address_type")]
    public string AddressType { get; set; } = string.Empty;

    [Column("city")]
    public string City { get; set; } = string.Empty;

    [Column("address_full")]
    public string AddressFull { get; set; } = string.Empty;

    [Column("region")]
    public string? Region { get; set; }

    [Column("zip_code")]
    public string? ZipCode { get; set; }

    // Navigation Properties
    [ForeignKey("PartnerId")]
    public Partner? Partner { get; set; }
}