using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Domain.Entities;

[Table("Order_status")]
public class OrderStatus : IEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    // Navigation Properties
    // public ICollection<Order> Orders { get; set; }
}