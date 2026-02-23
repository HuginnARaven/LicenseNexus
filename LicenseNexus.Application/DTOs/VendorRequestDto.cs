using FluentValidation;

namespace LicenseNexus.Application.DTOs;

public class VendorRequestDTO
{
    public string Name { get; set; } = string.Empty;
    public string? OriginalName { get; set; }
    public string? Description { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string? Logo { get; set; }
}

public class VendorRequestDTOValidator : AbstractValidator<VendorRequestDTO>
{
    public VendorRequestDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(255).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("CountryCode cannot be empty.")
            .Length(2, 3).WithMessage("CountryCode must be between 2 and 3 characters.");

        RuleFor(x => x.OriginalName)
            .MaximumLength(255).WithMessage("OriginalName cannot exceed 255 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
    }
}