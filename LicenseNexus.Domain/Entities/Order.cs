using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Domain.Entities;

[Table("Orders")]
public class Order : IEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; } // FK

    [Column("order_status_id")]
    public int OrderStatusId { get; set; } // FK

    [Column("check_date")]
    public DateTime CheckDate { get; set; } = DateTime.UtcNow;

    [Column("order_total_sum")]
    public decimal OrderTotalSum { get; set; }

    [Column("document_num")]
    public string DocumentNum { get; set; } = string.Empty;

    [Column("posting_date")]
    public DateTime? PostingDate { get; set; }

    [Column("invoice_requested")]
    public bool InvoiceRequested { get; set; }

    // Navigation Properties
    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }

    [ForeignKey("OrderStatusId")]
    public OrderStatus? OrderStatus { get; set; }
    
    public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}