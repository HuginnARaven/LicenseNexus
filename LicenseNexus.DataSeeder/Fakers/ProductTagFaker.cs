using LicenseNexus.Domain.Entities;

namespace LicenseNexus.DataSeeder.Fakers;

public sealed class ProductTagFaker(List<Tag> tags)
{
    public List<ProductTag> GenerateForProduct(int productId, int count = 0)
    {
        var random = new Random();
        if (count <= 0) count = random.Next(1, 4);
        var selectedTags = tags.OrderBy(_ => random.Next()).Take(count).ToList();
        
        return selectedTags.Select(t => new ProductTag
        {
            ProductId = productId,
            TagId = t.Id
        }).ToList();
    }
}