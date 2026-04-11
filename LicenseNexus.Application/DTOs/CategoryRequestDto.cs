using FluentValidation;

namespace LicenseNexus.Application.DTOs;

public class CategoryRequestDto
{
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public string Author { get; set; } = string.Empty;
}

public class CategoryRequestDtoValidator : AbstractValidator<CategoryRequestDto>
{
    public CategoryRequestDtoValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(150).WithMessage("Category name cannot exceed 100 characters.");
        RuleFor(x => x.Description)
            .MaximumLength(1024).WithMessage("Description cannot exceed 1024 characters.");
        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author name is required.")
            .MaximumLength(255).WithMessage("Author name cannot exceed 255 characters.");
    }
}