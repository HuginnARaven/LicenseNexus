using FluentValidation;

namespace LicenseNexus.Application.DTOs;

public class ProductGroupEditRequestDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Note { get; set; }
    public string Author { get; set; } = string.Empty;
}

public class ProductGroupEditRequestDtoValidator : AbstractValidator<ProductGroupEditRequestDto>
{
    public ProductGroupEditRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Note cannot exceed 500 characters.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(100).WithMessage("Author cannot exceed 100 characters.");
    }
}