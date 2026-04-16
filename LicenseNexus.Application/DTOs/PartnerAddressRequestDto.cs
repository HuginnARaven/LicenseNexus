using FluentValidation;
using LicenseNexus.Domain.Interfaces;

namespace LicenseNexus.Application.DTOs;

public class PartnerAddressRequestDto
{
    public int PartnerId { get; set; }
    public string City { get; set; } = string.Empty;
    public string AddressFull { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? ZipCode { get; set; }
}

public class PartnerAddressRequestDtoValidator : AbstractValidator<PartnerAddressRequestDto>
{
    public PartnerAddressRequestDtoValidator(IPartnerRepository partnerRepository)
    {
        RuleFor(x => x.PartnerId)
            .MustAsync(async (id, cancellation) => await partnerRepository.ExistsAsync(id, cancellation))
            .NotEmpty().WithMessage("PartnerId is required.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(80).WithMessage("City cannot exceed 80 characters.");

        RuleFor(x => x.AddressFull)
            .NotEmpty().WithMessage("AddressFull is required.")
            .MaximumLength(250).WithMessage("AddressFull cannot exceed 250 characters.");

        RuleFor(x => x.Region)
            .MaximumLength(80).WithMessage("Region cannot exceed 80 characters.");

        RuleFor(x => x.ZipCode)
            .MaximumLength(80).WithMessage("ZipCode cannot exceed 80 characters.");
    }
}