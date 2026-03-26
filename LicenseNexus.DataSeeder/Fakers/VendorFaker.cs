using Bogus;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.DataSeeder.Fakers;

public sealed class VendorFaker : Faker<Vendor>
{
    public VendorFaker() : base("en")
    {
        RuleFor(v => v.Name, f => f.Company.CompanyName());
        RuleFor(v => v.OriginalName, f => f.Company.CompanyName());
        RuleFor(v => v.Description, f => f.Company.CatchPhrase());
        RuleFor(v => v.CountryCode, f => f.Address.CountryCode(Bogus.DataSets.Iso3166Format.Alpha3));
        RuleFor(v => v.Logo, f => f.Image.PicsumUrl());
    }
}