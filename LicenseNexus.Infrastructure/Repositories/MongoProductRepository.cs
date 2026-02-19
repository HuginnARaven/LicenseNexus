using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoProductRepository : IProductRepository
{
    private readonly IMongoCollection<ProductDocument> _collection;
    private readonly MongoContext _context;

    public MongoProductRepository(MongoContext context)
    {
        _collection = context.Products;
        _context = context;
    }

    public async Task<ProductModel?> GetByIdAsync(int id)
    {
        var doc = await _collection.Find(x => x.ProductId == id).Limit(100).FirstAsync();
        return MapToModel(doc);
    }

    public async Task<IEnumerable<ProductModel>> GetAllAsync()
    {
        var docs = await _collection.Find(_ => true).Limit(100).ToListAsync();
        return docs.Select(MapToModel);
    }

    public async Task<PaginatedResult<ProductModel>> GetPaginatedAsync(
        int page, int pageSize, 
        int? categoryId, int? groupId, 
        int? vendorId, string? search,
        double? priceFrom, double? priceTo)
    {
        var builder = Builders<ProductDocument>.Filter;
        var filter = builder.Empty;

        if (categoryId.HasValue)
        {
            filter &= builder.Eq(x => x.Classification.Group.CategoryId, categoryId.Value);
        }

        if (groupId.HasValue)
        {
            filter &= builder.Eq(x => x.Classification.Group.Id, groupId.Value);
        }

        if (vendorId.HasValue)
        {
            filter &= builder.Eq(x => x.Classification.Vendor.Id, vendorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            filter &= builder.Regex(x => x.Title, new MongoDB.Bson.BsonRegularExpression(search, "i")) |
                      builder.Regex(x => x.Sku, new MongoDB.Bson.BsonRegularExpression(search, "i"));
        }

        if (priceFrom.HasValue || priceTo.HasValue)
        {
            var priceFilter = Builders<ProductPriceDoc>.Filter.Empty;
            if (priceFrom.HasValue)
            {
                priceFilter &= Builders<ProductPriceDoc>.Filter.Gte(p => p.Price, (decimal)priceFrom.Value);
            }
            if (priceTo.HasValue)
            {
                priceFilter &= Builders<ProductPriceDoc>.Filter.Lte(p => p.Price, (decimal)priceTo.Value);
            }
            filter &= builder.ElemMatch(x => x.Prices, priceFilter);
        }

        var totalCount = await _collection.CountDocumentsAsync(filter);
        
        var documents = await _collection.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return new PaginatedResult<ProductModel>
        {
            Items = documents.Select(MapToModel).ToList(),
            TotalCount = (int)totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductModel?> AddAsync(ProductModel product)
    {
        var id = await _context.GetNextSequenceValueAsync("product_id");
        product.Id = id;
        var doc = MapToDocument(product);
        await _collection.InsertOneAsync(doc);
        return product;
    }

    public async Task UpdateAsync(ProductModel product)
    {
        var newDoc = MapToDocument(product);
        var oldDoc = await _collection.Find(x => x.ProductId == product.Id).Limit(100).FirstAsync();
        newDoc.InternalId = oldDoc.InternalId;
        await _collection.ReplaceOneAsync(x => x.ProductId == product.Id, newDoc);
    }

    public async Task DeleteAsync(int id)
    {
        await _collection.DeleteOneAsync(x => x.ProductId == id);
    }

    private ProductDocument MapToDocument(ProductModel model)
    {
        return new ProductDocument
        {
            ProductId = model.Id,
            Sku = model.Sku,
            Title = model.Title,
            IsActive = model.IsActive,
            Tags = model.Tags,
            Classification = new ClassificationDoc
            {
                TypeName = model.Classification.TypeName,
                UnitMeasureName = model.Classification.UnitMeasureName,
                Vendor = new VendorDoc
                {
                    Id = model.Classification.Vendor.Id,
                    Name = model.Classification.Vendor.Name,
                    CountryCode = model.Classification.Vendor.CountryCode
                },
                Group = new GroupDoc
                {
                    Id = model.Classification.Group.Id,
                    Name = model.Classification.Group.Name,
                    CategoryId = model.Classification.Group.CategoryId,
                    CategoryName = model.Classification.Group.CategoryName
                }
            },
            Attributes = new AttributesDoc
            {
                ShortDescription = model.Attributes.ShortDescription,
                QuantityMin = model.Attributes.QuantityMin,
                QuantityMax = model.Attributes.QuantityMax,
                IsPromo = model.Attributes.IsPromo,
                IsTop = model.Attributes.IsTop,
                IsNew = model.Attributes.IsNew,
                Logo = model.Attributes.Logo,
                StartDate = model.Attributes.StartDate,
                EndDate = model.Attributes.EndDate,
                CreatedDate = model.Attributes.CreatedDate,
                Author = model.Attributes.Author
            },
            Descriptions = model.Descriptions.Select(d => new DescriptionDoc
            {
                FullText = d.FullText,
                LanguageCode = d.LanguageCode
            }).ToList(),
            Currency = new CurrencyDoc
            {
                Id = model.Currency.Id,
                LiteralCode = model.Currency.LiteralCode,
                Name = model.Currency.Name
            },
            Prices = model.Prices.Select(p => new ProductPriceDoc
            {
                Price = p.Price,
                TermDuration = p.TermDuration,
                BillingPlan = p.BillingPlan,
                Segment = p.Segment,
                CountryCode = p.CountryCode,
                StartDate = p.StartDate
            }).ToList()
        };
    }

    private ProductModel MapToModel(ProductDocument doc)
    {
        return new ProductModel
        {
            Id = doc.ProductId,
            Sku = doc.Sku,
            Title = doc.Title,
            IsActive = doc.IsActive,
            Tags = doc.Tags,
            Classification = new ClassificationModel
            {
                TypeName = doc.Classification.TypeName,
                UnitMeasureName = doc.Classification.UnitMeasureName,
                Vendor = new VendorModel
                {
                    Id = doc.Classification.Vendor.Id,
                    Name = doc.Classification.Vendor.Name,
                    CountryCode = doc.Classification.Vendor.CountryCode
                },
                Group = new GroupModel
                {
                    Id = doc.Classification.Group.Id,
                    Name = doc.Classification.Group.Name,
                    CategoryId = doc.Classification.Group.CategoryId,
                    CategoryName = doc.Classification.Group.CategoryName
                }
            },
            Attributes = new AttributesModel
            {
                ShortDescription = doc.Attributes.ShortDescription ?? string.Empty,
                QuantityMin = doc.Attributes.QuantityMin,
                QuantityMax = doc.Attributes.QuantityMax,
                IsPromo = doc.Attributes.IsPromo,
                IsTop = doc.Attributes.IsTop,
                IsNew = doc.Attributes.IsNew,
                Logo = doc.Attributes.Logo,
                StartDate = doc.Attributes.StartDate,
                EndDate = doc.Attributes.EndDate,
                CreatedDate = doc.Attributes.CreatedDate,
                Author = doc.Attributes.Author
            },
            Descriptions = doc.Descriptions.Select(d => new DescriptionModel
            {
                FullText = d.FullText,
                LanguageCode = d.LanguageCode
            }).ToList(),
            Currency = new CurrencyModel
            {
                Id = doc.Currency.Id,
                LiteralCode = doc.Currency.LiteralCode,
                Name = doc.Currency.Name
            },
            Prices = doc.Prices.Select(p => new ProductPriceModel
            {
                Price = p.Price,
                TermDuration = p.TermDuration,
                BillingPlan = p.BillingPlan,
                Segment = p.Segment,
                CountryCode = p.CountryCode,
                StartDate = p.StartDate
            }).ToList()
        };
    }
}