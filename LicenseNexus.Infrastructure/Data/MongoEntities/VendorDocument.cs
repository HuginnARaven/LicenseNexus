using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicenseNexus.Infrastructure.Data.MongoEntities;

public class VendorDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InternalId { get; set; }

    [BsonElement("id")]
    public int Id { get; set; } //TODO: mb remove if unused

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("country_code")]
    public string CountryCode { get; set; } = string.Empty;
}