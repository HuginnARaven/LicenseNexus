using Bogus;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.DataSeeder.Fakers;

public sealed class CategoryFaker : Faker<Category>
{
    public CategoryFaker() : base("en")
    {
        RuleFor(c => c.IsActive, f => f.Random.Bool());
        RuleFor(c => c.CategoryName, f => f.Commerce.Categories(1)[0] + " " + f.Random.Guid().ToString().Substring(0, 5));
        RuleFor(c => c.Description, f => f.Lorem.Sentence());
        RuleFor(c => c.CreatedDate, f => f.Date.Past());
        RuleFor(c => c.Author, f => f.Internet.UserName());
    }
}