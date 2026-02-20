using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicenseNexus.Infrastructure.Data.MongoDocuments;

public class VendorDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InternalId { get; set; } = string.Empty;

    [BsonElement("id")]
    public int Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
    
    [BsonElement("original_name")]
    public string? OriginalName { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("country_code")]
    public string CountryCode { get; set; } = string.Empty;
    
    [BsonElement("logo")]
    public string? Logo { get; set; }
}