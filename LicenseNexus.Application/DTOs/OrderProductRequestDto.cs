using FluentValidation;

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

public class OrderProductRequestDtoValidator : AbstractValidator<OrderProductRequestDto>
{
    public OrderProductRequestDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.PriceId)
            .NotEmpty().WithMessage("PriceId is required.");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.CustomerPrice)
            .GreaterThanOrEqualTo(0).WithMessage("CustomerPrice must be 0 or greater.");

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters.");
    }
}
