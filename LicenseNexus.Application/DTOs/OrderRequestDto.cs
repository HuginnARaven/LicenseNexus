namespace LicenseNexus.Application.DTOs;

public class OrderRequestDto
{
    public int CustomerId { get; set; }
    public int OrderStatusId { get; set; }
    public required string DocumentNum { get; set; }
    public DateTime? PostingDate { get; set; }
    public bool InvoiceRequested { get; set; }
}