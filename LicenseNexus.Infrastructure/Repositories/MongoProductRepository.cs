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

    public async Task PatchAsync(int id, ProductPatchFields updates)
    {
        var updateDefinitions = new List<UpdateDefinition<ProductDocument>>();
        var builder = Builders<ProductDocument>.Update;

        if (!string.IsNullOrWhiteSpace(updates.Sku))
            updateDefinitions.Add(builder.Set(x => x.Sku, updates.Sku));
        
        if (!string.IsNullOrWhiteSpace(updates.Title))
            updateDefinitions.Add(builder.Set(x => x.Title, updates.Title));
        
        if (!string.IsNullOrWhiteSpace(updates.ShortDescription))
            updateDefinitions.Add(builder.Set(x => x.Attributes.ShortDescription, updates.ShortDescription));
        
        if (updates.QuantityMin.HasValue)
            updateDefinitions.Add(builder.Set(x => x.Attributes.QuantityMin, updates.QuantityMin.Value));
        
        if (updates.QuantityMax.HasValue)
            updateDefinitions.Add(builder.Set(x => x.Attributes.QuantityMin, updates.QuantityMax.Value));
        
        if (updates.StartDate.HasValue)
            updateDefinitions.Add(builder.Set(x => x.Attributes.StartDate, updates.StartDate));
        
        if (updates.EndDate.HasValue)
            updateDefinitions.Add(builder.Set(x => x.Attributes.EndDate, updates.EndDate));
        
        if (updates.IsPromo.HasValue)
            updateDefinitions.Add(builder.Set(x => x.Attributes.IsPromo, updates.IsPromo));
        
        if (updates.IsTop.HasValue)
            updateDefinitions.Add(builder.Set(x => x.Attributes.IsTop, updates.IsTop));
        
        if (updates.IsNew.HasValue)
            updateDefinitions.Add(builder.Set(x => x.Attributes.IsNew, updates.IsNew));
        
        if (!string.IsNullOrWhiteSpace(updates.Logo))
            updateDefinitions.Add(builder.Set(x => x.Attributes.Logo, updates.Logo));
        
        if (!string.IsNullOrWhiteSpace(updates.Author))
            updateDefinitions.Add(builder.Set(x => x.Attributes.Author, updates.Author));
        
        if (updates.VendorId.HasValue)
        {
            var vendor = await _context.Vendors.Find(v => v.Id == updates.VendorId).FirstOrDefaultAsync();
            if (vendor != null)
            {
                var vendorSubset = new VendorDoc
                { 
                    Id = vendor.Id, 
                    Name = vendor.Name, 
                    CountryCode = vendor.CountryCode 
                };
                updateDefinitions.Add(builder.Set(x => x.Classification.Vendor, vendorSubset));
            }
        }
        
        if (updates.ProductTypeId.HasValue)
        {
            var productType = await _context.ProductTypes.Find(v => v.Id == updates.ProductTypeId).FirstOrDefaultAsync();
            if (productType != null)
            {
                updateDefinitions.Add(builder.Set(x => x.Classification.TypeId, productType.Id));
                updateDefinitions.Add(builder.Set(x => x.Classification.TypeName, productType.TypeName));
            }
        }
        
        if (updates.UnitMeasureId.HasValue)
        {
            var unitMeasure = await _context.UnitMeasures.Find(v => v.Id == updates.UnitMeasureId).FirstOrDefaultAsync();
            if (unitMeasure != null)
            {
                updateDefinitions.Add(builder.Set(x => x.Classification.UnitMeasureId, unitMeasure.Id));
                updateDefinitions.Add(builder.Set(x => x.Classification.UnitMeasureName, unitMeasure.Name));
            }
        }
        
        if (updates.CurrencyId.HasValue)
        {
            var currency = await _context.Currencies.Find(v => v.Id == updates.CurrencyId).FirstOrDefaultAsync();
            if (currency != null)
            {
                var currencySubset = new CurrencyDoc()
                { 
                    Id = currency.Id,
                    LiteralCode = currency.LiteralCode,
                    Name = currency.Name
                };
                updateDefinitions.Add(builder.Set(x => x.Currency, currencySubset));
            }
        }
        
        if (updates.ProductGroupId.HasValue)
        {
            var filter = Builders<CategoryDocument>.Filter.ElemMatch(c => c.Groups, g => g.Id == updates.ProductGroupId);
            var category = await _context.Categories.Find(filter).FirstOrDefaultAsync();
            var productGroup = category.Groups.FirstOrDefault(g => g.Id == id);
            if (category != null && productGroup != null)
            {
                var GroupSubset = new GroupDoc()
                { 
                    Id = productGroup.Id,
                    Name = productGroup.Name,
                    CategoryId = category.Id,
                    CategoryName = category.Name
                };
                updateDefinitions.Add(builder.Set(x => x.Classification.Group, GroupSubset));
            }
        }

        if (updateDefinitions.Any())
        {
            var combinedUpdate = builder.Combine(updateDefinitions);
            await _collection.UpdateOneAsync(x => x.ProductId == id, combinedUpdate);
        }
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