using Bogus;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.DataSeeder.Fakers;

public sealed class FullDescriptionFaker : Faker<FullDescription>
{
    public FullDescriptionFaker(): base("en")
    {
        RuleFor(fd => fd.FullText, f => f.Lorem.Paragraphs(3));
        RuleFor(fd => fd.LanguageCode, "en");
    }
    
    public List<FullDescription> GenerateForProduct(int productId, int count = 1)
    {
        var descriptions = Generate(count);
        foreach (var desc in descriptions)
        {
            desc.ProductId = productId;
        }
        return descriptions;
    }
}