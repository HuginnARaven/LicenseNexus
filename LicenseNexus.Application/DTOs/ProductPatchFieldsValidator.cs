using FluentValidation;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;

namespace LicenseNexus.Application.DTOs;

public class ProductPatchFieldsValidator : AbstractValidator<ProductPatchFields>
{
    public ProductPatchFieldsValidator(
        IVendorRepository vendorRepository, 
        IProductGroupRepository productGroupRepository,
        IProductTypeRepository productTypeRepository,
        IUnitMeasureRepository unitMeasureRepository,
        ICurrencyRepository currencyRepository)
    {
        RuleFor(x => x.Title)
            .MaximumLength(250).WithMessage("Title cannot exceed 250 characters.")
            .When(x => x.Title != null);
        
        RuleFor(x => x.ShortDescription)
            .MaximumLength(250).WithMessage("ShortDescription cannot exceed 250 characters.")
            .When(x => x.ShortDescription != null);

        RuleFor(x => x.QuantityMin)
            .GreaterThanOrEqualTo(0).WithMessage("QuantityMin must be 0 or greater.")
            .When(x => x.QuantityMin.HasValue);

        RuleFor(x => x.QuantityMax)
            .GreaterThanOrEqualTo(x => x.QuantityMin ?? 0).WithMessage("QuantityMax must be greater than or equal to QuantityMin.")
            .When(x => x.QuantityMax.HasValue);

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("StartDate must be less than or equal to EndDate.");

        RuleFor(x => x.Author)
            .MaximumLength(100).WithMessage("Author cannot exceed 100 characters.")
            .When(x => x.Author != null);
        
        RuleFor(x => x.VendorId)
            .MustAsync(async (id, cancellation) => await vendorRepository.ExistsAsync(id!.Value, cancellation))
            .When(x => x.VendorId.HasValue)
            .WithMessage("Vendor with the specified ID does not exist.");

        RuleFor(x => x.ProductGroupId)
            .MustAsync(async (id, cancellation) => await productGroupRepository.ExistsAsync(id!.Value, cancellation))
            .When(x => x.ProductGroupId.HasValue)
            .WithMessage("Product Group with the specified ID does not exist.");
        
        RuleFor(x => x.ProductTypeId)
            .MustAsync(async (id, cancellation) => await productTypeRepository.ExistsAsync(id!.Value, cancellation))
            .When(x => x.ProductTypeId.HasValue)
            .WithMessage("Product Type with the specified ID does not exist.");

        RuleFor(x => x.UnitMeasureId)
            .MustAsync(async (id, cancellation) => await unitMeasureRepository.ExistsAsync(id!.Value, cancellation))
            .When(x => x.UnitMeasureId.HasValue)
            .WithMessage("Unit Measure with the specified ID does not exist.");

        RuleFor(x => x.CurrencyId)
            .MustAsync(async (id, cancellation) => await currencyRepository.ExistsAsync(id!.Value, cancellation))
            .When(x => x.UnitMeasureId.HasValue)
            .WithMessage("Currency with the specified ID does not exist.");
    }
}