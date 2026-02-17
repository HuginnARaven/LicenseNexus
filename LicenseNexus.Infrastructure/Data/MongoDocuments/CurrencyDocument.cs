using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicenseNexus.Infrastructure.Data.MongoDocuments;

public class CurrencyDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InternalId { get; set; } = string.Empty;

    [BsonElement("id")]
    public int Id { get; set; }
    
    [BsonElement("literal_code")]
    public string LiteralCode { get; set; } = string.Empty; // USD, UAH
    
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
    
    [BsonElement("country_code")]
    public string CountryCode { get; set; } = string.Empty;
}