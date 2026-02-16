using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicenseNexus.Infrastructure.Data.MongoDocuments;

public class CartDocument
{
    [BsonId] 
    [BsonRepresentation(BsonType.Int32)]
    public int UserId { get; set; }

    [BsonElement("items")]
    public List<CartItemDoc> Items { get; set; } = new();
    
}

public class CartItemDoc
{
    [BsonElement("product_id")]
    public int ProductId { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; }
}