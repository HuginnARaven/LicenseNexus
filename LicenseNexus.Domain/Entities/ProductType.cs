using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseNexus.Domain.Entities;

[Table("Product_type")]
public class ProductType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("type_name")]
    [Required]
    public string TypeName { get; set; } = string.Empty;
}