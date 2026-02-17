using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicenseNexus.Infrastructure.Data.MongoDocuments;

public class ProductTypeDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InternalId { get; set; } = string.Empty;
    
    [BsonElement("id")]
    public int Id { get; set; }
    
    [BsonElement("type_name")]
    public string TypeName { get; set; } = string.Empty;
}