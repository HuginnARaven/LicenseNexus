using Bogus;
using Bogus.Extensions.UnitedStates;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.DataSeeder.Fakers;

public class PartnerFaker : Faker<Partner>
{
    public PartnerFaker() : base("en")
    {
        RuleFor(p => p.Status, f => f.PickRandom("Active", "Inactive", "Pending"));
        RuleFor(p => p.FullCompanyName, f => f.Company.CompanyName());
        RuleFor(p => p.RegistrationNumber, f => f.Company.Ein());
        RuleFor(p => p.TaxNumber, f => f.Finance.Iban());
        RuleFor(p => p.BankAccountNumber, f => f.Finance.Account());
        RuleFor(p => p.BankName, f => f.Company.CompanyName() + " Bank");
        RuleFor(p => p.Phone, f => f.Phone.PhoneNumber());
        RuleFor(p => p.CreatedDate, f => f.Date.Past());
        RuleFor(p => p.Author, f => f.Name.FullName());
        RuleFor(v => v.CountryCode, f => f.Address.CountryCode(Bogus.DataSets.Iso3166Format.Alpha3));
    }
}