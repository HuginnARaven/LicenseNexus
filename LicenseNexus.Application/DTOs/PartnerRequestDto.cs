using FluentValidation;

namespace LicenseNexus.Application.DTOs;

public class PartnerRequestDto
{
    public string CountryCode { get; set; } = string.Empty;
    public string FullCompanyName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Author { get; set; } = string.Empty;
}

public class PartnerRequestDtoValidator : AbstractValidator<PartnerRequestDto>
{
    public PartnerRequestDtoValidator()
    {
        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("CountryCode is required.")
            .Length(2, 3).WithMessage("CountryCode must be between 2 and 3 characters.");

        RuleFor(x => x.FullCompanyName)
            .NotEmpty().WithMessage("FullCompanyName is required.")
            .MaximumLength(255).WithMessage("FullCompanyName cannot exceed 255 characters.");

        RuleFor(x => x.RegistrationNumber)
            .NotEmpty().WithMessage("RegistrationNumber is required.")
            .MaximumLength(20).WithMessage("RegistrationNumber cannot exceed 20 characters.");

        RuleFor(x => x.TaxNumber)
            .NotEmpty().WithMessage("TaxNumber is required.")
            .MaximumLength(50).WithMessage("TaxNumber cannot exceed 50 characters.");

        RuleFor(x => x.BankAccountNumber)
            .NotEmpty().WithMessage("BankAccountNumber is required.")
            .MaximumLength(50).WithMessage("BankAccountNumber cannot exceed 50 characters.");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("BankName is required.")
            .MaximumLength(255).WithMessage("BankName cannot exceed 255 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(30).WithMessage("Phone cannot exceed 30 characters.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(200).WithMessage("Author cannot exceed 200 characters.");
    }
}
