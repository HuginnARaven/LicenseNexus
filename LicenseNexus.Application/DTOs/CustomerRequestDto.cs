using FluentValidation;

namespace LicenseNexus.Application.DTOs;

public class CustomerRequestDto
{
    public int PartnerId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? ZipCode { get; set; }
    public string CountryCode { get; set; } = string.Empty;
}

public class CustomerRequestDtoValidator : AbstractValidator<CustomerRequestDto>
{
    public CustomerRequestDtoValidator()
    {
        RuleFor(x => x.PartnerId)
            .NotEmpty().WithMessage("PartnerId is required.");

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("AccountName is required.")
            .MaximumLength(255).WithMessage("AccountName cannot exceed 255 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.LegalName)
            .NotEmpty().WithMessage("LegalName is required.")
            .MaximumLength(255).WithMessage("LegalName cannot exceed 255 characters.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("LegalName is required.")
            .Length(2, 3).WithMessage("CountryCode must be between 2 and 3 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.Region)
            .MaximumLength(100).WithMessage("Region cannot exceed 100 characters.");

        RuleFor(x => x.ZipCode)
            .MaximumLength(20).WithMessage("ZipCode cannot exceed 20 characters.");
    }
}
