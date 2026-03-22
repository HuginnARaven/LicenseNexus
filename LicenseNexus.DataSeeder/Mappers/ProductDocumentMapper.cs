using LicenseNexus.Domain.Entities;
using LicenseNexus.Infrastructure.Data.MongoDocuments;

namespace LicenseNexus.DataSeeder.Mappers;

public static class ProductDocumentMapper
{
    public static ProductDocument Map(
        Product product, 
        List<Tag> allTags,
        List<ProductGroup> allGroups,
        List<Vendor> allVendors,
        List<ProductType> allTypes,
        List<UnitMeasure> allMeasures,
        List<Currency> allCurrencies)
    {
        var group = allGroups.FirstOrDefault(g => g.Id == product.ProductGroupId);
        var vendor = allVendors.FirstOrDefault(v => v.Id == product.VendorId);
        var type = allTypes.FirstOrDefault(t => t.Id == product.ProductTypeId);
        var measure = allMeasures.FirstOrDefault(m => m.Id == product.UnitMeasureId);
        var currency = allCurrencies.FirstOrDefault(c => c.Id == product.CurrencyId);

        var doc = new ProductDocument
        {
            ProductId = product.Id,
            Sku = product.Sku ?? string.Empty,
            Title = product.Title,
            IsActive = group!.IsActive && group.Category!.IsActive,
            Tags = product.ProductTags.Select(pt => 
            {
                var t = allTags.FirstOrDefault(x => x.Id == pt.TagId);
                return new TagDoc { Id = t?.Id ?? 0, Name = t?.Name ?? "" };
            }).ToList(),
            Classification = new ClassificationDoc
            {
                TypeId = type?.Id ?? 0,
                TypeName = type?.TypeName ?? "",
                UnitMeasureId = measure?.Id ?? 0,
                UnitMeasureName = measure?.Name ?? "",
                Vendor = new VendorDoc 
                { 
                    Id = vendor?.Id ?? 0, 
                    Name = vendor?.Name ?? "",
                    CountryCode = vendor?.CountryCode
                },
                Group = new GroupDoc
                {
                    Id = group?.Id ?? 0,
                    Name = group?.Name ?? "",
                    CategoryId = group?.CategoryId ?? 0,
                    CategoryName = group?.Category?.CategoryName ?? ""
                }
            },
            Attributes = new AttributesDoc
            {
                ShortDescription = product.ShortDescription,
                QuantityMin = product.QuantityMin,
                QuantityMax = product.QuantityMax,
                IsPromo = product.IsPromo,
                IsTop = product.IsTop,
                IsNew = product.IsNew,
                Logo = product.Logo,
                StartDate = product.StartDate,
                EndDate = product.EndDate,
                CreatedDate = product.CreatedDate,
                Author = product.Author
            },
            Descriptions = product.FullDescriptions.Select(d => new DescriptionDoc
            {
                Id = d.Id,
                FullText = d.FullText,
                LanguageCode = d.LanguageCode
            }).ToList(),
            Currency = new CurrencyDoc
            {
                Id = currency?.Id ?? 0,
                LiteralCode = currency?.LiteralCode ?? "",
                Name = currency?.Name ?? ""
            },
            Prices = product.Prices.Select(p => new ProductPriceDoc
            {
                Id = p.Id,
                Price = p.Price,
                TermDuration = p.TermDuration,
                BillingPlan = p.BillingPlan,
                Segment = p.Segment,
                CountryCode = p.CountryCode,
                StartDate = p.StartDate
            }).ToList()
        };

        return doc;
    }
}