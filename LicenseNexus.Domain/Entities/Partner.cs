using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LicenseNexus.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LicenseNexus.Domain.Entities;

[Table("Partner")]
[Index(nameof(RegistrationNumber), IsUnique = true)]
[Index(nameof(TaxNumber), IsUnique = true)]
[Index(nameof(BankAccountNumber), IsUnique = true)]
public class Partner : IEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("country_code")]
    [StringLength(3)]
    public string? CountryCode { get; set; }

    [Column("full_company_name")]
    [Required]
    public string FullCompanyName { get; set; } = string.Empty;

    [Column("registration_number")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Column("tax_number")]
    public string TaxNumber { get; set; } = string.Empty;

    [Column("bank_account_number")]
    public string BankAccountNumber { get; set; } = string.Empty;

    [Column("bank_name")]
    public string BankName { get; set; } = string.Empty;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("autor")]
    public string Author { get; set; } = string.Empty;
    
    // Navigation Properties
    public ICollection<PartnerAddress> Addresses { get; set; } = new List<PartnerAddress>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}