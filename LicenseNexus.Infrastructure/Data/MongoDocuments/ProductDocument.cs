using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicenseNexus.Infrastructure.Data.MongoDocuments;

public class ProductDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string InternalId { get; set; } = string.Empty;

    [BsonElement("id")]
    public int ProductId { get; set; } // for sync with Orders
    
    [BsonElement("sku")]
    public string Sku { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("active_is")]
    public bool IsActive { get; set; }
    
    [BsonElement("tags")]
    public List<TagDoc> Tags { get; set; } = new();
    
    [BsonElement("classification")]
    public ClassificationDoc Classification { get; set; } = new();
    
    [BsonElement("attributes")]
    public AttributesDoc Attributes { get; set; } = new();
    
    [BsonElement("description")]
    public List<DescriptionDoc> Descriptions { get; set; } = new();
    
    [BsonElement("currency")]
    public CurrencyDoc Currency { get; set; } = new();
    
    [BsonElement("prices")]
    public List<ProductPriceDoc> Prices { get; set; } = new();
}

public class TagDoc
{
    [BsonElement("id")]
    public int Id { get; set; }
    
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
}

public class ClassificationDoc
{
    [BsonElement("type_id")]
    public int TypeId { get; set; }

    [BsonElement("type_name")]
    public string TypeName { get; set; } = string.Empty;

    [BsonElement("unit_measure_id")]
    public int UnitMeasureId { get; set; }

    [BsonElement("unit_measure_name")]
    public string UnitMeasureName { get; set; } = string.Empty;

    [BsonElement("vendor")]
    public VendorDoc Vendor { get; set; } = new();

    [BsonElement("group")]
    public GroupDoc Group { get; set; } = new();
}

public class VendorDoc
{
    [BsonElement("id")]
    public int Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("country_code")]
    public string CountryCode { get; set; } = string.Empty;
}

public class GroupDoc
{
    [BsonElement("id")]
    public int Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
    
    [BsonElement("category_id")]
    public int CategoryId { get; set; }

    [BsonElement("category_name")]
    public string CategoryName { get; set; } = string.Empty;
}

public class AttributesDoc
{
    [BsonElement("short_description")]
    public string? ShortDescription { get; set; }
    
    [BsonElement("quantity_min")]
    public int QuantityMin { get; set; }

    [BsonElement("quantity_max")]
    public int QuantityMax { get; set; }

    [BsonElement("is_promo")]
    public bool IsPromo { get; set; }

    [BsonElement("is_top")]
    public bool IsTop { get; set; }

    [BsonElement("is_new")]
    public bool IsNew { get; set; }

    [BsonElement("logo")]
    public string? Logo { get; set; }

    [BsonElement("start_date")]
    public DateTime? StartDate { get; set; }

    [BsonElement("end_date")]
    public DateTime? EndDate { get; set; }

    [BsonElement("created_date")]
    public DateTime CreatedDate { get; set; }

    [BsonElement("autor")]
    public string? Author { get; set; }
}

public class DescriptionDoc
{
    [BsonElement("full_text")]
    public string FullText { get; set; } = string.Empty;

    [BsonElement("languages_code")]
    public string LanguageCode { get; set; } = string.Empty;
}

public class ProductPriceDoc
{
    [BsonElement("id")]
    public int Id { get; set; }
    
    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; }

    [BsonElement("term_duration")]
    public int? TermDuration { get; set; }

    [BsonElement("billing_plan")]
    public string? BillingPlan { get; set; }

    [BsonElement("segment")]
    public string? Segment { get; set; }

    [BsonElement("country_code")]
    public string? CountryCode { get; set; }

    [BsonElement("start_date")]
    public DateTime? StartDate { get; set; }
}

public class CurrencyDoc
{
    [BsonElement("id")]
    public int Id { get; set; }

    [BsonElement("literal_code")]
    public string LiteralCode { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
}
