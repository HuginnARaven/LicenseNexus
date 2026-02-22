namespace LicenseNexus.Application.DTOs;

public class OrderProductRequestDto
{
    public int ProductId { get; set; }
    public int PriceId { get; set; }
    public int OrderId { get; set; } 
    public int Quantity { get; set; }
    public decimal CustomerPrice { get; set; }
    public string? Status { get; set; }
}