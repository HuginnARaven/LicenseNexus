using FluentValidation;

namespace LicenseNexus.Application.DTOs;

public class OrderRequestDto
{
    public int CustomerId { get; set; }
    public int OrderStatusId { get; set; }
    public DateTime? PostingDate { get; set; }
    public bool InvoiceRequested { get; set; } = false;
}

public class OrderRequestDtoValidator : AbstractValidator<OrderRequestDto>
{
    public OrderRequestDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.OrderStatusId)
            .NotEmpty().WithMessage("OrderStatusId is required.");

        RuleFor(x => x.PostingDate)
            .NotEmpty().WithMessage("PostingDate is required.");
    }
}
