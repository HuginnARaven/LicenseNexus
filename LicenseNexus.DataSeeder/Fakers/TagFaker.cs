using Bogus;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.DataSeeder.Fakers;

public class TagFaker : Faker<Tag>
{
    public TagFaker() : base("en")
    {
        RuleFor(t => t.Name, f => $"{f.Commerce.ProductAdjective()}_{f.Random.AlphaNumeric(6)}");
    }

    public List<Tag> GenerateUnique(int count)
    {
        return Generate(count).DistinctBy(t => t.Name).ToList();
    }
}