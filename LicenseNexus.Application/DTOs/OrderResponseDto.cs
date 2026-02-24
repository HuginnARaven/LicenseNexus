using LicenseNexus.Domain.Enums;

namespace LicenseNexus.Application.DTOs;

public class OrderResponseDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int OrderStatusId { get; set; }
    public decimal OrderTotalSum { get; set; }
    public required string DocumentNum { get; set; }
    public DateTime? PostingDate { get; set; }
    public bool InvoiceRequested { get; set; }
    public List<OrderProductResponseDto> OrderProducts { get; set; } = [];
}

public class OrderProductResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? CustomerPrice { get; set; }
    public decimal PartnerPrice { get; set; }
    public decimal SumTotal { get; set; }
    public ChargeType? ChargeType { get; set; }
    public string? TermDuration { get; set; }
    public string? BillingCycle { get; set; }
    public string? Status { get; set; }
}
