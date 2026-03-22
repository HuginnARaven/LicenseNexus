using Bogus;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.DataSeeder.Fakers;

public class ProductFaker : Faker<Product>
{
    public ProductFaker(
        List<Vendor> vendors, 
        List<ProductType> types, 
        List<UnitMeasure> measures, 
        List<Currency> currencies, 
        List<ProductGroup> groups) : base("en")
    {
        RuleFor(p => p.Sku, f => f.Commerce.Ean13()); 
        RuleFor(p => p.Title, f => f.Commerce.ProductName()); 
        RuleFor(p => p.ShortDescription, f => f.Lorem.Sentence()); 
        RuleFor(p => p.VendorId, f => f.PickRandom(vendors).Id);
        RuleFor(p => p.ProductTypeId, f => f.PickRandom(types).Id);
        RuleFor(p => p.UnitMeasureId, f => f.PickRandom(measures).Id);
        RuleFor(p => p.CurrencyId, f => f.PickRandom(currencies).Id);
        RuleFor(p => p.ProductGroupId, f => f.PickRandom(groups).Id);
        RuleFor(p => p.QuantityMin, f => f.Random.Int(1, 10)); 
        RuleFor(p => p.QuantityMax, f => f.Random.Int(100, 1000)); 
        RuleFor(p => p.StartDate, f => f.Date.Past()); 
        RuleFor(p => p.EndDate, f => f.Date.Future()); 
        RuleFor(p => p.IsPromo, f => f.Random.Bool()); 
        RuleFor(p => p.IsTop, f => f.Random.Bool()); 
        RuleFor(p => p.IsNew, f => f.Random.Bool()); 
        RuleFor(p => p.Logo, f => f.Image.PicsumUrl()); 
        RuleFor(p => p.CreatedDate, f => f.Date.Past()); 
        RuleFor(p => p.Author, f => f.Internet.UserName());
    }
}