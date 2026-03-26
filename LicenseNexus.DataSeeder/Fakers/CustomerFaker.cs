using Bogus;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.DataSeeder.Fakers;

public sealed class CustomerFaker : Faker<Customer>
{
    public CustomerFaker() : base("en")
    {
        RuleFor(c => c.AccountName, f => f.Internet.UserName() + f.Random.Number(100, 999));
        RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.AccountName));
        RuleFor(c => c.LegalName, f => f.Company.CompanyName());
        RuleFor(c => c.City, f => f.Address.City());
        RuleFor(c => c.Region, f => f.Address.State());
        RuleFor(c => c.ZipCode, f => f.Address.ZipCode());
        RuleFor(c => c.CountryCode, f => f.Address.CountryCode(Bogus.DataSets.Iso3166Format.Alpha3));
        RuleFor(c => c.Status, f => f.PickRandom("Active", "Inactive", "Suspended"));
        RuleFor(c => c.CreatedDate, f => f.Date.Past());
    }
    
    public List<Customer> GenerateForPartner(int partnerId, int count = 1)
    {
        var customers = Generate(count);
        foreach (var customer in customers)
        {
            customer.PartnerId = partnerId;
        }
        return customers;
    }
}