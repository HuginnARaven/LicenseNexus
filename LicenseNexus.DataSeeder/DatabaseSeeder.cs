using Bogus;
using LicenseNexus.Domain.Entities;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Text.Json;

namespace LicenseNexus.DataSeeder;

public class DatabaseSeeder
{
    private readonly ExtendedSqlContext _extendedSqlContext;
    private readonly MongoContext _mongoContext;
    private readonly RedisContext _redisContext;

    public DatabaseSeeder(
        ExtendedSqlContext extendedSqlContext,
        MongoContext mongoContext,
        RedisContext redisContext)
    {
        _extendedSqlContext = extendedSqlContext;
        _mongoContext = mongoContext;
        _redisContext = redisContext;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Generating Data...");

        // 1. Clear existing data (optional, be careful in production!)
        await ClearDataAsync();

        // 2. Generate and Save Vendors
        var vendors = GenerateVendors(10);
        await _extendedSqlContext.Vendors.AddRangeAsync(vendors);
        await _extendedSqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {vendors.Count} vendors to SQL.");

        var vendorDocs = vendors.Select(v => new VendorDocument
        {
            Id = v.Id,
            Name = v.Name,
            OriginalName = v.OriginalName,
            Description = v.Description,
            CountryCode = v.CountryCode,
            Logo = v.Logo
        }).ToList();
        if (vendorDocs.Any())
        {
            await _mongoContext.Vendors.InsertManyAsync(vendorDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "vendor_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, vendors.Max(v => v.Id)),
                new UpdateOptions { IsUpsert = true });
            Console.WriteLine($"Saved {vendorDocs.Count} vendors to Mongo.");
        }

        // 3. Generate and Save Categories & ProductGroups
        var categories = GenerateCategories(5);
        await _extendedSqlContext.Categories.AddRangeAsync(categories);
        await _extendedSqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {categories.Count} categories to SQL.");

        var categoryDocs = new List<CategoryDocument>();
        var maxProdutGroupId = 0;
        foreach (var cat in categories)
        {
            var groups = GenerateProductGroups(3, cat.Id);
            await _extendedSqlContext.ProductGroups.AddRangeAsync(groups);
            await _extendedSqlContext.SaveChangesAsync(); // Save groups to get IDs
            
            cat.ProductGroups = groups; // Link for local usage if needed

            var groupDocs = groups.Select(g => new ProductGroupDoc
            {
                Id = g.Id,
                Name = g.Name,
                IsActive = g.IsActive,
                Note = g.Note,
                CreatedDate = g.CreatedDate,
                Author = g.Author
            }).ToList();

            categoryDocs.Add(new CategoryDocument
            {
                Id = cat.Id,
                IsActive = cat.IsActive,
                Name = cat.CategoryName,
                Description = cat.Description,
                CreatedDate = cat.CreatedDate,
                Groups = groupDocs
            });
            maxProdutGroupId = Math.Max(maxProdutGroupId, groups.Any() ? groups.Max(pg => pg.Id) : 0);
        }
        if (categoryDocs.Any())
        {
            await _mongoContext.Categories.InsertManyAsync(categoryDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "category_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, categories.Max(с => с.Id)),
                new UpdateOptions { IsUpsert = true }
            );
            if (maxProdutGroupId > 0)
                await _mongoContext.Counters.UpdateOneAsync(
                    ds => ds.Id == "product_group_id", 
                    Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, maxProdutGroupId),
                    new UpdateOptions { IsUpsert = true }
                );
            Console.WriteLine($"Saved {categoryDocs.Count} categories to Mongo.");
        }

        // 4. Generate and Save ProductTypes
        var productTypes = GenerateProductTypes();
        await _extendedSqlContext.ProductTypes.AddRangeAsync(productTypes);
        await _extendedSqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {productTypes.Count} product types to SQL.");

        var productTypeDocs = productTypes.Select(pt => new ProductTypeDocument
        {
            Id = pt.Id,
            TypeName = pt.TypeName
        }).ToList();
        if (productTypeDocs.Any())
        {
            await _mongoContext.ProductTypes.InsertManyAsync(productTypeDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "product_type_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, productTypes.Max(с => с.Id)),
                new UpdateOptions { IsUpsert = true }
            );
            Console.WriteLine($"Saved {productTypeDocs.Count} product types to Mongo.");
        }

        // 5. Generate and Save UnitMeasures
        var unitMeasures = GenerateUnitMeasures();
        await _extendedSqlContext.UnitMeasures.AddRangeAsync(unitMeasures);
        await _extendedSqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {unitMeasures.Count} unit measures to SQL.");

        var unitMeasureDocs = unitMeasures.Select(um => new UnitMeasureDocument
        {
            Id = um.Id,
            Name = um.Name
        }).ToList();
        if (unitMeasureDocs.Any())
        {
            await _mongoContext.UnitMeasures.InsertManyAsync(unitMeasureDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "unit_measure_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, unitMeasures.Max(с => с.Id)),
                new UpdateOptions { IsUpsert = true }
            );
            Console.WriteLine($"Saved {unitMeasureDocs.Count} unit measures to Mongo.");
        }

        // 6. Generate and Save Currencies
        var currencies = GenerateCurrencies();
        await _extendedSqlContext.Currencies.AddRangeAsync(currencies);
        await _extendedSqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {currencies.Count} currencies to SQL.");

        var currencyDocs = currencies.Select(c => new CurrencyDocument
        {
            Id = c.Id,
            LiteralCode = c.LiteralCode,
            Name = c.Name,
            CountryCode = c.CountryCode
        }).ToList();
        if (currencyDocs.Any())
        {
            await _mongoContext.Currencies.InsertManyAsync(currencyDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "currency_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, currencies.Max(с => с.Id)),
                new UpdateOptions { IsUpsert = true }
            );
            Console.WriteLine($"Saved {currencyDocs.Count} currencies to Mongo.");
        }

        // 7. Generate and Save Tags
        var tags = GenerateTags(20);
        await _extendedSqlContext.Tags.AddRangeAsync(tags);
        await _extendedSqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {tags.Count} tags to SQL.");

        var tagDocs = tags.Select(t => new TagDocument
        {
            Id = t.Id,
            Name = t.Name
        }).ToList();
        if (tagDocs.Any())
        {
            await _mongoContext.Tags.InsertManyAsync(tagDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "tag_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, tags.Max(с => с.Id)),
                new UpdateOptions { IsUpsert = true }
            );
            Console.WriteLine($"Saved {tagDocs.Count} tags to Mongo.");
        }

        // 8. Generate and Save Products
        var allGroups = await _extendedSqlContext.ProductGroups.Include(g => g.Category).ToListAsync();
        
        var products = GenerateProducts(10, vendors, productTypes, unitMeasures, currencies, allGroups);
        
        await _extendedSqlContext.Products.AddRangeAsync(products);
        await _extendedSqlContext.SaveChangesAsync();
        Console.WriteLine($"Saved {products.Count} products to SQL.");

        // Generate related data for products (Prices, Descriptions, Tags)
        var productDocs = new List<ProductDocument>();
        var redisDb = _redisContext.Database;

        var maxPriceId = 0;
        var maxDescriptionId = 0;
        
        foreach (var product in products)
        {
            // Prices
            var prices = GenerateProductPrices(product.Id);
            await _extendedSqlContext.ProductPrices.AddRangeAsync(prices);
            await _extendedSqlContext.SaveChangesAsync();
            product.Prices = prices;
            maxPriceId = prices.Max(p => p.Id) > maxPriceId ? prices.Max(p => p.Id) : maxPriceId;

            // Descriptions
            var descriptions = GenerateFullDescriptions(product.Id);
            await _extendedSqlContext.FullDescriptions.AddRangeAsync(descriptions);
            await _extendedSqlContext.SaveChangesAsync();
            product.FullDescriptions = descriptions;
            maxDescriptionId = descriptions.Max(d => d.Id) > maxDescriptionId ? descriptions.Max(d => d.Id) : maxDescriptionId;

            // Product Tags (Many-to-Many)
            var productTags = GenerateProductTags(product.Id, tags);
            await _extendedSqlContext.ProductTags.AddRangeAsync(productTags);
            await _extendedSqlContext.SaveChangesAsync();
            product.ProductTags = productTags;

            // Prepare Mongo Document
            var pDoc = MapToProductDocument(product, prices, descriptions, productTags, tags, allGroups, vendors, productTypes, unitMeasures, currencies);
            productDocs.Add(pDoc);

            // Save to Redis
            //string redisKey = $"product:{product.Id}";
            //string redisValue = JsonSerializer.Serialize(pDoc);
            //await redisDb.StringSetAsync(redisKey, redisValue);
        }

        await _extendedSqlContext.SaveChangesAsync();
        Console.WriteLine("Saved product related data (prices, descriptions, tags) to SQL.");

        if (productDocs.Any())
        {
            await _mongoContext.Products.InsertManyAsync(productDocs);
            await _mongoContext.Counters.UpdateOneAsync(
                ds => ds.Id == "product_id", 
                Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, products.Max(с => с.Id)),
                new UpdateOptions { IsUpsert = true }
            );
            if (maxPriceId > 0)
                await _mongoContext.Counters.UpdateOneAsync(
                    ds => ds.Id == "product_price_id", 
                    Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, maxPriceId),
                    new UpdateOptions { IsUpsert = true }
                );
            if (maxDescriptionId > 0)
                await _mongoContext.Counters.UpdateOneAsync(
                    ds => ds.Id == "product_description_id", 
                    Builders<DatabaseSequence>.Update.Set(ds => ds.Seq, maxDescriptionId),
                    new UpdateOptions { IsUpsert = true }
                );

            Console.WriteLine($"Saved {productDocs.Count} products to Mongo.");
        }
    }

    private ProductDocument MapToProductDocument(
        Product product, 
        List<ProductPrice> prices, 
        List<FullDescription> descriptions, 
        List<ProductTag> productTags,
        List<LicenseNexus.Domain.Entities.Tag> allTags,
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
            Tags = productTags.Select(pt => 
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
            Descriptions = descriptions.Select(d => new DescriptionDoc
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
            Prices = prices.Select(p => new ProductPriceDoc
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

    private List<Vendor> GenerateVendors(int count)
    {
        var faker = new Faker<Vendor>("en")
            .RuleFor(v => v.Name, f => f.Company.CompanyName())
            .RuleFor(v => v.OriginalName, f => f.Company.CompanyName())
            .RuleFor(v => v.Description, f => f.Company.CatchPhrase())
            .RuleFor(v => v.CountryCode, f => f.Address.CountryCode(Bogus.DataSets.Iso3166Format.Alpha3))
            .RuleFor(v => v.Logo, f => f.Image.PicsumUrl());

        return faker.Generate(count);
    }

    private List<Category> GenerateCategories(int count)
    {
        var faker = new Faker<Category>("en")
            .RuleFor(c => c.IsActive, f => f.Random.Bool())
            .RuleFor(c => c.CategoryName, f => f.Commerce.Categories(1)[0] + " " + f.Random.Guid().ToString().Substring(0, 5))
            .RuleFor(c => c.Description, f => f.Lorem.Sentence())
            .RuleFor(c => c.CreatedDate, f => f.Date.Past())
            .RuleFor(c => c.Author, f => f.Internet.UserName());

        return faker.Generate(count);
    }

    private List<ProductGroup> GenerateProductGroups(int count, int categoryId)
    {
        var faker = new Faker<ProductGroup>("en")
            .RuleFor(g => g.Name, f => f.Commerce.Department() + " " + f.Random.Guid().ToString().Substring(0, 5))
            .RuleFor(g => g.IsActive, f => f.Random.Bool())
            .RuleFor(g => g.Note, f => f.Lorem.Sentence())
            .RuleFor(g => g.CreatedDate, f => f.Date.Past())
            .RuleFor(g => g.Author, f => f.Internet.UserName())
            .RuleFor(g => g.CategoryId, categoryId);

        return faker.Generate(count);
    }

    private List<ProductType> GenerateProductTypes()
    {
        return new List<ProductType>
        {
            new ProductType { TypeName = "License" },
            new ProductType { TypeName = "Subscription" },
            new ProductType { TypeName = "Service" },
            new ProductType { TypeName = "Physical Good" }
        };
    }

    private List<UnitMeasure> GenerateUnitMeasures()
    {
        return new List<UnitMeasure>
        {
            new UnitMeasure { Name = "pcs" },
            new UnitMeasure { Name = "users" },
            new UnitMeasure { Name = "months" },
            new UnitMeasure { Name = "years" }
        };
    }

    private List<Currency> GenerateCurrencies()
    {
        return new List<Currency>
        {
            new Currency { LiteralCode = "USD", Name = "US Dollar", CountryCode = "USA" },
            new Currency { LiteralCode = "EUR", Name = "Euro", CountryCode = "EUR" },
            new Currency { LiteralCode = "UAH", Name = "Hryvnia", CountryCode = "UKR" }
        };
    }

    private List<LicenseNexus.Domain.Entities.Tag> GenerateTags(int count)
    {
        var faker = new Faker<LicenseNexus.Domain.Entities.Tag>("en")
            .RuleFor(t => t.Name, f => f.Commerce.ProductAdjective());

        return faker.Generate(count).DistinctBy(t => t.Name).ToList();
    }

    private List<Product> GenerateProducts(
        int count, 
        List<Vendor> vendors, 
        List<ProductType> types, 
        List<UnitMeasure> measures, 
        List<Currency> currencies, 
        List<ProductGroup> groups)
    {
        var faker = new Faker<Product>("en")
            .RuleFor(p => p.Sku, f => f.Commerce.Ean13())
            .RuleFor(p => p.Title, f => f.Commerce.ProductName())
            .RuleFor(p => p.ShortDescription, f => f.Lorem.Sentence())
            .RuleFor(p => p.VendorId, f => f.PickRandom(vendors).Id)
            .RuleFor(p => p.ProductTypeId, f => f.PickRandom(types).Id)
            .RuleFor(p => p.UnitMeasureId, f => f.PickRandom(measures).Id)
            .RuleFor(p => p.CurrencyId, f => f.PickRandom(currencies).Id)
            .RuleFor(p => p.ProductGroupId, f => f.PickRandom(groups).Id)
            .RuleFor(p => p.QuantityMin, f => f.Random.Int(1, 10))
            .RuleFor(p => p.QuantityMax, f => f.Random.Int(100, 1000))
            .RuleFor(p => p.StartDate, f => f.Date.Past())
            .RuleFor(p => p.EndDate, f => f.Date.Future())
            .RuleFor(p => p.IsPromo, f => f.Random.Bool())
            .RuleFor(p => p.IsTop, f => f.Random.Bool())
            .RuleFor(p => p.IsNew, f => f.Random.Bool())
            .RuleFor(p => p.Logo, f => f.Image.PicsumUrl())
            .RuleFor(p => p.CreatedDate, f => f.Date.Past())
            .RuleFor(p => p.Author, f => f.Internet.UserName());

        return faker.Generate(count);
    }

    private List<ProductPrice> GenerateProductPrices(int productId)
    {
        var faker = new Faker<ProductPrice>("en")
            .RuleFor(pp => pp.ProductId, productId)
            .RuleFor(pp => pp.Price, f => f.Finance.Amount(10, 1000))
            .RuleFor(pp => pp.TermDuration, f => f.PickRandom("1 Month", "1 Year", "Lifetime"))
            .RuleFor(pp => pp.BillingPlan, f => f.PickRandom("Monthly", "Yearly", "One-time"))
            .RuleFor(pp => pp.CountryCode, f => f.Address.CountryCode(Bogus.DataSets.Iso3166Format.Alpha3))
            .RuleFor(pp => pp.Segment, f => f.PickRandom("B2B", "B2C", "Gov"))
            .RuleFor(pp => pp.StartDate, f => f.Date.Past());

        return faker.Generate(new Random().Next(1, 4));
    }

    private List<FullDescription> GenerateFullDescriptions(int productId)
    {
        var faker = new Faker<FullDescription>("en")
            .RuleFor(fd => fd.ProductId, productId)
            .RuleFor(fd => fd.FullText, f => f.Lorem.Paragraphs(3))
            .RuleFor(fd => fd.LanguageCode, "en");

        return faker.Generate(1);
    }

    private List<ProductTag> GenerateProductTags(int productId, List<LicenseNexus.Domain.Entities.Tag> tags)
    {
        var random = new Random();
        var selectedTags = tags.OrderBy(_ => random.Next()).Take(random.Next(1, 4)).ToList();
        
        return selectedTags.Select(t => new ProductTag
        {
            ProductId = productId,
            TagId = t.Id
        }).ToList();
    }
    
    private async Task ClearDataAsync()
    {
        Console.WriteLine("Clearing data...");
        
        await _extendedSqlContext.OrderProducts.ExecuteDeleteAsync();
        await _extendedSqlContext.Orders.ExecuteDeleteAsync();
        await _extendedSqlContext.ProductTags.ExecuteDeleteAsync();
        await _extendedSqlContext.ProductPrices.ExecuteDeleteAsync();

        await _extendedSqlContext.Products.ExecuteDeleteAsync();
        await _extendedSqlContext.ProductGroups.ExecuteDeleteAsync();
        
        await _extendedSqlContext.Vendors.ExecuteDeleteAsync();
        await _extendedSqlContext.Categories.ExecuteDeleteAsync();
        await _extendedSqlContext.Tags.ExecuteDeleteAsync();
        await _extendedSqlContext.UnitMeasures.ExecuteDeleteAsync();
        await _extendedSqlContext.ProductTypes.ExecuteDeleteAsync();
        await _extendedSqlContext.Currencies.ExecuteDeleteAsync();

        Console.WriteLine("MSSQL cleared.");
        
        var mongoEmptyFilter = Builders<VendorDocument>.Filter.Empty;

        await _mongoContext.Vendors.DeleteManyAsync(_ => true);
        await _mongoContext.Categories.DeleteManyAsync(_ => true);
        await _mongoContext.Products.DeleteManyAsync(_ => true);
        await _mongoContext.Tags.DeleteManyAsync(_ => true);
        await _mongoContext.UnitMeasures.DeleteManyAsync(_ => true);
        await _mongoContext.ProductTypes.DeleteManyAsync(_ => true);
        await _mongoContext.Currencies.DeleteManyAsync(_ => true);
        
        await _mongoContext.Counters.DeleteManyAsync(_ => true); 

        Console.WriteLine("MongoDB cleared.");
        
        // var endpoints = _redisMultiplexer.GetEndPoints();
        // var server = _redisMultiplexer.GetServer(endpoints.First());
        // await server.FlushDatabaseAsync(); // Повністю очищає поточну базу Redis
        // Console.WriteLine("Redis cleared.");
    }
}