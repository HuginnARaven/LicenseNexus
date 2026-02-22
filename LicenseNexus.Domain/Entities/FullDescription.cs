using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Domain.Entities;

[Table("Full_description")]
public class FullDescription : IEntity
{
    [Key] [Column("id")] 
    public int Id { get; set; }

    [Column("product_id")] 
    public int ProductId { get; set; }

    [Column("full_text")] 
    public string FullText { get; set; } = string.Empty;

    [Column("languages_code")] 
    public string LanguageCode { get; set; } = "en";

    // Navigation Properties
    [ForeignKey("ProductId")] 
    public Product? Product { get; set; }
}