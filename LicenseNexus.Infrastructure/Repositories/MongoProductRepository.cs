using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Domain.Models;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using MongoDB.Driver;

namespace LicenseNexus.Infrastructure.Repositories;

public class MongoProductRepository : IProductRepository
{
    private readonly IMongoCollection<ProductDocument> _collection;

    public MongoProductRepository(MongoContext context)
    {
        _collection = context.Products;
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

    public async Task AddAsync(ProductModel product)
    {
        var doc = MapToDocument(product);
        await _collection.InsertOneAsync(doc);
    }

    public async Task UpdateAsync(ProductModel product)
    {
        var doc = MapToDocument(product);
        await _collection.ReplaceOneAsync(x => x.ProductId == product.Id, doc);
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
            CurrencyCode = model.CurrencyCode,
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
            Classification = new Classification
            {
                TypeName = doc.Classification.TypeName,
                UnitMeasureName = doc.Classification.UnitMeasureName,
                Vendor = new Vendor
                {
                    Id = doc.Classification.Vendor.Id,
                    Name = doc.Classification.Vendor.Name,
                    CountryCode = doc.Classification.Vendor.CountryCode
                },
                Group = new Group
                {
                    Id = doc.Classification.Group.Id,
                    Name = doc.Classification.Group.Name,
                    CategoryId = doc.Classification.Group.CategoryId,
                    CategoryName = doc.Classification.Group.CategoryName
                }
            },
            Attributes = new Attributes
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
            Descriptions = doc.Descriptions.Select(d => new Description
            {
                FullText = d.FullText,
                LanguageCode = d.LanguageCode
            }).ToList(),
            CurrencyCode = doc.CurrencyCode,
            Prices = doc.Prices.Select(p => new ProductPrice
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