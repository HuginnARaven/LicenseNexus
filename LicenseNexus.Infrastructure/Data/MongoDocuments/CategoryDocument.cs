using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicenseNexus.Infrastructure.Data.MongoDocuments;

public class CategoryDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InternalId { get; set; } = string.Empty;

    [BsonElement("id")]
    public int Id { get; set; }
    
    [BsonElement("active_is")]
    public bool IsActive { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    [BsonElement("groups")]
    public List<ProductGroupDoc> Groups { get; set; } = new();
}

public class ProductGroupDoc
{
    [BsonElement("id")]
    public int Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("active_is")]
    public bool IsActive { get; set; }
    
    [BsonElement("note")]
    public string? Note { get; set; }

    [BsonElement("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [BsonElement("autor")]
    public string Author { get; set; } = string.Empty;
}