using FluentValidation;

namespace LicenseNexus.Application.DTOs;

public class CurrencyRequestDto
{
    public string LiteralCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}

public class CurrencyRequestDtoValidator : AbstractValidator<CurrencyRequestDto>
{    public CurrencyRequestDtoValidator()
    {
        RuleFor(x => x.LiteralCode)
            .NotEmpty().WithMessage("LiteralCode cannot be empty.")
            .Length(3).WithMessage("LiteralCode must be exactly 3 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(20).WithMessage("Name cannot exceed 20 characters.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("CountryCode cannot be empty.")
            .Length(2, 3).WithMessage("CountryCode must be between 2 and 3 characters.");
    }
}
