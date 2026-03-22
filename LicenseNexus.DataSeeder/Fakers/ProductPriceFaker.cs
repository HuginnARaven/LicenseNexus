using Bogus;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.DataSeeder.Fakers;

public class ProductPriceFaker: Faker<ProductPrice>
{
    public ProductPriceFaker() : base("en")
    {
        RuleFor(pp => pp.Price, f => f.Finance.Amount(10, 2000));
        RuleFor(pp => pp.TermDuration, f => f.PickRandom("1 Month", "1 Year", "Lifetime"));
        RuleFor(pp => pp.BillingPlan, f => f.PickRandom("Monthly", "Yearly", "One-time"));
        RuleFor(pp => pp.CountryCode, f => f.Address.CountryCode(Bogus.DataSets.Iso3166Format.Alpha3));
        RuleFor(pp => pp.Segment, f => f.PickRandom("B2B", "B2C", "Gov"));
        RuleFor(pp => pp.StartDate, f => f.Date.Past());
    }

    public List<ProductPrice> GenerateForProduct(int productId, int count = 0)
    {
        if (count <= 0) count = new Random().Next(1, 4);
        var prices = Generate(count);
        foreach (var price in prices)
        {
            price.ProductId = productId;
        }
        return prices;
    }
}