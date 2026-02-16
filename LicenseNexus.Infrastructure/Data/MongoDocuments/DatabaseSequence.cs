using MongoDB.Bson.Serialization.Attributes;

namespace LicenseNexus.Infrastructure.Data.MongoDocuments;

public class DatabaseSequence
{
    [BsonId]
    public string Id { get; set; } = string.Empty; // sequence name (ex. "product_id")

    [BsonElement("seq")]
    public int Seq { get; set; }
}