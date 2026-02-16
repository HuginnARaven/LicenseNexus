namespace LicenseNexus.Infrastructure.Data.RedisEntities;

public class ProductModel
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    public List<string> Tags { get; set; } = new();

    public ClassificationModel Classification { get; set; } = new();
    public AttributesModel Attributes { get; set; } = new();
        
    public List<DescriptionModel> Descriptions { get; set; } = new();
    public List<ProductPriceModel> Prices { get; set; } = new();
}