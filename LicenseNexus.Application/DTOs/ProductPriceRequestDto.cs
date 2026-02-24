using FluentValidation;

namespace LicenseNexus.Application.DTOs;

public class ProductPriceRequestDto
{
    public decimal Price { get; set; }
    public string? TermDuration { get; set; }
    public string? BillingPlan { get; set; }
    public string? CountryCode { get; set; }
    public string? Segment { get; set; }
    public DateTime StartDate { get; set; }
}

public class ProductPriceRequestDtoValidator : AbstractValidator<ProductPriceRequestDto>
{
    public ProductPriceRequestDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .NotNull().WithMessage("Start Date cannot be null.");
        
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater or equal to 0.");

        RuleFor(x => x.CountryCode)
            .MaximumLength(3).WithMessage("CountryCode cannot exceed 3 characters.");

        RuleFor(x => x.BillingPlan)
            .MaximumLength(20).WithMessage("BillingPlan cannot exceed 20 characters.");

        RuleFor(x => x.Segment)
            .MaximumLength(20).WithMessage("Segment cannot exceed 20 characters.");
    }
}