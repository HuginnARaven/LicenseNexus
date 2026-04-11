using FluentValidation;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.DTOs;

public class ProductRequestDto
{
    
    public string Sku { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public int VendorId { get; set; }
    public int ProductTypeId { get; set; }
    public int UnitMeasureId { get; set; }
    public int CurrencyId { get; set; }
    public int ProductGroupId { get; set; }
    public int QuantityMin { get; set; }
    public int QuantityMax { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsPromo { get; set; } = false;
    public bool IsTop { get; set; } = false;
    public bool IsNew { get; set; }
    public string? Logo { get; set; }
    public string Author { get; set; } = string.Empty;
}

public class ProductRequestDtoValidator : AbstractValidator<ProductRequestDto>
{
    public ProductRequestDtoValidator(
        IVendorRepository vendorRepository, 
        IProductGroupRepository productGroupRepository,
        IProductTypeRepository productTypeRepository,
        IUnitMeasureRepository unitMeasureRepository,
        ICurrencyRepository currencyRepository)
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("Sku cannot be empty.")
            .MaximumLength(100).WithMessage("Sku cannot exceed 100 characters.");
        
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty.")
            .MaximumLength(250).WithMessage("Title cannot exceed 250 characters.");
        
        RuleFor(x => x.ShortDescription)
            .NotEmpty().WithMessage("ShortDescription cannot be empty.")
            .MaximumLength(250).WithMessage("ShortDescription cannot exceed 250 characters.");

        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage("VendorId cannot be empty.");
        
        RuleFor(x => x.UnitMeasureId)
            .NotEmpty().WithMessage("UnitMeasureId cannot be empty.");
        
        RuleFor(x => x.CurrencyId)
            .NotEmpty().WithMessage("CurrencyId cannot be empty.");

        RuleFor(x => x.ProductGroupId)
            .NotEmpty().WithMessage("ProductGroupId cannot be empty.");

        RuleFor(x => x.QuantityMin)
            .GreaterThanOrEqualTo(0).WithMessage("QuantityMin must be 0 or greater.");

        RuleFor(x => x.QuantityMax)
            .GreaterThanOrEqualTo(x => x.QuantityMin).WithMessage("QuantityMax must be greater than or equal to QuantityMin.");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("StartDate must be less than or equal to EndDate.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate must be greater than or equal to StartDate.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author cannot be empty.")
            .MaximumLength(200).WithMessage("Author cannot exceed 200 characters.");
        
        RuleFor(x => x.Logo)
            .MaximumLength(128).WithMessage("Author cannot exceed 128 characters.");
        
        RuleFor(x => x.VendorId)
            .MustAsync(async (id, cancellation) => await vendorRepository.ExistsAsync(id, cancellation))
            .WithMessage("Vendor with the specified ID does not exist.");

        RuleFor(x => x.ProductGroupId)
            .MustAsync(async (id, cancellation) => await productGroupRepository.ExistsAsync(id, cancellation))
            .WithMessage("Product Group with the specified ID does not exist.");
        
        RuleFor(x => x.ProductTypeId)
            .MustAsync(async (id, cancellation) => await productTypeRepository.ExistsAsync(id, cancellation))
            .WithMessage("Product Type with the specified ID does not exist.");

        RuleFor(x => x.UnitMeasureId)
            .MustAsync(async (id, cancellation) => await unitMeasureRepository.ExistsAsync(id, cancellation))
            .WithMessage("Unit Measure with the specified ID does not exist.");

        RuleFor(x => x.CurrencyId)
            .MustAsync(async (id, cancellation) => await currencyRepository.ExistsAsync(id, cancellation))
            .WithMessage("Currency with the specified ID does not exist.");
    }
}