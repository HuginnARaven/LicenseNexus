using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseNexus.Domain.Enums;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Domain.Entities;

[Table("Order_product")]
public class OrderProduct : IEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; } // FK

    [Column("product_id")]
    public int ProductId { get; set; } // Logical Link in case of mongo configuration

    [Column("quantity")]
    public int Quantity { get; set; }
    
    [Column("customer_price")]
    public decimal? CustomerPrice { get; set; }

    [Column("partner_price")]
    public decimal PartnerPrice { get; set; }

    [Column("sum_total")]
    public decimal SumTotal { get; set; }

    [Column("charge_type")]
    public ChargeType? ChargeType { get; set; }

    [Column("term_duration")]
    public string? TermDuration { get; set; } = string.Empty;

    [Column("billing_cycle")]
    public string? BillingCycle { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    // Navigation Properties
    [ForeignKey("OrderId")]
    public Order? Order { get; set; }
}