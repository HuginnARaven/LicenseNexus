using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Domain.Entities;

[Table("Product_type")]
public class ProductType : IEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("type_name")]
    [Required]
    public string TypeName { get; set; } = string.Empty;
}