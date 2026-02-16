using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicenseNexus.Infrastructure.Data.MongoDocuments;

public class CategoryDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InternalId { get; set; }

    [BsonElement("id")]
    public int Id { get; set; } // TODO: mb remove if unused

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("active_is")]
    public bool IsActive { get; set; }
    
    [BsonElement("groups")]
    public List<ProductGroupDoc> Groups { get; set; } = new();
}

public class ProductGroupDoc
{
    [BsonElement("id")]
    public int Id { get; set; }  // TODO: mb remove if unused

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("active_is")]
    public bool IsActive { get; set; }
}