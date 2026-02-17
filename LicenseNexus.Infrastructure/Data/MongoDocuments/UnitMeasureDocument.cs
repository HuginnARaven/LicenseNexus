using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicenseNexus.Infrastructure.Data.MongoDocuments;

public class UnitMeasureDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InternalId { get; set; } = string.Empty;
    
    [BsonElement("id")]
    public int Id { get; set; }
    
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
}