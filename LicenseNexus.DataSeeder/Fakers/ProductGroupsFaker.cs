using Bogus;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.DataSeeder.Fakers;

public sealed class ProductGroupsFaker : Faker<ProductGroup>
{
    public ProductGroupsFaker() : base("en")
    {
        RuleFor(g => g.Name, f => f.Commerce.Department() + " " + f.Random.Guid().ToString().Substring(0, 5));
        RuleFor(g => g.IsActive, f => f.Random.Bool());
        RuleFor(g => g.Note, f => f.Lorem.Sentence());
        RuleFor(g => g.CreatedDate, f => f.Date.Past());
        RuleFor(g => g.Author, f => f.Internet.UserName());
    }
    
    public List<ProductGroup> GenerateForCategory(int categoryId, int count = 1)
    {
        var productGroups = Generate(count);
        foreach (var pd in productGroups)
        {
            pd.CategoryId = categoryId;
        }
        return productGroups;
    }
}