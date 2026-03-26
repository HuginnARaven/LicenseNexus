using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Data.MongoDocuments;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using LicenseNexus.DataSeeder.Seeders;

namespace LicenseNexus.DataSeeder;

public class DatabaseSeeder
{
    private readonly ExtendedSqlContext _extendedSqlContext;
    private readonly BaseSqlContext _baseSqlContext;
    private readonly MongoContext _mongoContext;
    private readonly RedisContext _redisContext;
    
    private readonly ProductSeeder _productSeeder;
    private readonly VendorSeeder _vendorSeeder;
    private readonly CategorySeeder _categorySeeder;
    private readonly ProductTypeSeeder _productTypeSeeder;
    private readonly UnitMeasureSeeder _unitMeasureSeeder;
    private readonly CurrencySeeder _currencySeeder;
    private readonly TagSeeder _tagSeeder;
    private readonly PartnerSeeder _partnerSeeder;


    public DatabaseSeeder(
        ExtendedSqlContext extendedSqlContext,
        BaseSqlContext baseSqlContext,
        MongoContext mongoContext,
        RedisContext redisContext,
        ProductSeeder productSeeder,
        VendorSeeder vendorSeeder,
        CategorySeeder categorySeeder,
        ProductTypeSeeder productTypeSeeder,
        UnitMeasureSeeder unitMeasureSeeder,
        CurrencySeeder currencySeeder,
        TagSeeder tagSeeder,
        PartnerSeeder partnerSeeder)
    {
        _extendedSqlContext = extendedSqlContext;
        _baseSqlContext = baseSqlContext;
        _mongoContext = mongoContext;
        _redisContext = redisContext;
        _productSeeder = productSeeder;
        _vendorSeeder = vendorSeeder;
        _categorySeeder = categorySeeder;
        _productTypeSeeder = productTypeSeeder;
        _unitMeasureSeeder = unitMeasureSeeder;
        _currencySeeder = currencySeeder;
        _tagSeeder = tagSeeder;
        _partnerSeeder = partnerSeeder;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("Generating Data...");

        // const int vendorsCount = 300;
        // const int categoriesCount = 20;
        // const int groupsPerCategoryCount = 8;
        // const int tagsCount = 200;
        // const int productsCount = 100000;
        // const int partnersCount = 15;
        
        const int vendorsCount = 5;
        const int categoriesCount = 5;
        const int groupsPerCategoryCount = 3;
        const int tagsCount = 10;
        const int productsCount = 1000;
        const int partnersCount = 3;
        
        // 1. Clear existing data
        await ClearDataAsync();

        // 2. Generate and Save Vendors
        await _vendorSeeder.SeedAsync(vendorsCount);
        var vendors = await _extendedSqlContext.Vendors.ToListAsync();

        // 3. Generate and Save Categories & ProductGroups
        await _categorySeeder.SeedAsync(categoriesCount, groupsPerCategoryCount);
        var allGroups = await _extendedSqlContext.ProductGroups.Include(g => g.Category).ToListAsync();
        
        // 4. Generate and Save ProductTypes
        await _productTypeSeeder.SeedAsync();
        var productTypes = await _extendedSqlContext.ProductTypes.ToListAsync();

        // 5. Generate and Save UnitMeasures
        await _unitMeasureSeeder.SeedAsync();
        var unitMeasures = await _extendedSqlContext.UnitMeasures.ToListAsync();

        // 6. Generate and Save Currencies
        await _currencySeeder.SeedAsync();
        var currencies = await _extendedSqlContext.Currencies.ToListAsync();

        // 7. Generate and Save Tags
        await _tagSeeder.SeedAsync(tagsCount);
        var tags = await _extendedSqlContext.Tags.ToListAsync();

        // 8. Generate and Save Products
        await _productSeeder.SeedAsync(
            productsCount, 
            500, 
            vendors, 
            productTypes, 
            unitMeasures, 
            currencies, 
            allGroups, 
            tags);
        
        // 9. Generate and Save Partners, Addresses, and Customers
        await _partnerSeeder.SeedAsync(partnersCount);
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
        
        await _extendedSqlContext.Customers.ExecuteDeleteAsync();
        await _extendedSqlContext.PartnerAddresses.ExecuteDeleteAsync();
        await _extendedSqlContext.Partners.ExecuteDeleteAsync();

        Console.WriteLine("MSSQL Full_Db cleared.");
        
        await _baseSqlContext.OrderProducts.ExecuteDeleteAsync();
        await _baseSqlContext.Orders.ExecuteDeleteAsync();
        
        await _baseSqlContext.Customers.ExecuteDeleteAsync();
        await _baseSqlContext.PartnerAddresses.ExecuteDeleteAsync();
        await _baseSqlContext.Partners.ExecuteDeleteAsync();
        
        Console.WriteLine("MSSQL Minimal_Db cleared.");
        
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
        // await server.FlushDatabaseAsync();
        // Console.WriteLine("Redis cleared.");
    }
}