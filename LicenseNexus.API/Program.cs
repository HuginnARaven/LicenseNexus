using FluentValidation;
using LicenseNexus.Application.DTOs;
using LicenseNexus.Application.Interfaces;
using LicenseNexus.Application.Services;
using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Repositories;
using LicenseNexus.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var mongoConnString = builder.Configuration.GetConnectionString("MongoArchDatabase");
var redisConnString = builder.Configuration.GetConnectionString("RedisArchDatabase");

builder.Services.AddDbContext<BaseSqlContext>(options => options.UseSqlServer(mongoConnString));
builder.Services.AddDbContext<ExtendedSqlContext>(options => options.UseSqlServer(redisConnString));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings")); // mb need to change MongoDbSettings to MongoContext.MongoDbSettings?
builder.Services.AddScoped<MongoContext>();
builder.Services.AddSingleton<RedisContext>();
builder.Services.AddSingleton<InMemoryEventBus>();
builder.Services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<InMemoryEventBus>());
builder.Services.AddHostedService<EventProcessorBackgroundService>();

var archMode = builder.Configuration["ArchitectureMode"];

if (archMode == "Redis")
{
    builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<ExtendedSqlContext>());
    builder.Services.AddScoped<BaseSqlContext>(provider => provider.GetRequiredService<ExtendedSqlContext>());
    
    builder.Services.AddScoped<ICartRepository, RedisCartRepository>();
    builder.Services.AddScoped<IProductRepository, RedisProductRepository>();
    builder.Services.AddScoped<IVendorRepository, RedisVendorRepository>();
    builder.Services.AddScoped<ICategoryRepository, RedisCategoryRepository>();
    builder.Services.AddScoped<IProductGroupRepository, RedisProductGroupRepository>();
    builder.Services.AddScoped<ICurrencyRepository, RedisCurrencyRepository>();
    builder.Services.AddScoped<IUnitMeasureRepository, RedisUnitMeasureRepository>();
    builder.Services.AddScoped<IProductTypeRepository, RedisProductTypeRepository>();
    builder.Services.AddScoped<ITagRepository, RedisTagRepository>();
    
    builder.Services.AddScoped<IProductCacheService, ProductCacheService>();
    builder.Services.AddScoped<IProductSyncService, RedisProductSyncService>();
}
else // Mongo
{
    builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<BaseSqlContext>());
    
    builder.Services.AddScoped<ICartRepository, MongoCartRepository>();
    builder.Services.AddScoped<IProductRepository, MongoProductRepository>();
    builder.Services.AddScoped<IVendorRepository, MongoVendorRepository>();
    builder.Services.AddScoped<ICategoryRepository, MongoCategoryRepository>();
    builder.Services.AddScoped<IProductGroupRepository, MongoProductGroupRepository>();
    builder.Services.AddScoped<ICurrencyRepository, MongoCurrencyRepository>();
    builder.Services.AddScoped<IUnitMeasureRepository, MongoUnitMeasureRepository>();
    builder.Services.AddScoped<IProductTypeRepository, MongoProductTypeRepository>();
    builder.Services.AddScoped<ITagRepository, MongoTagRepository>();
    
    builder.Services.AddScoped<IProductSyncService, MongoProductSyncService>();
}

builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.AddScoped<IPartnerRepository, SqlPartnerRepository>();
builder.Services.AddScoped<ICustomerRepository, SqlCustomerRepository>();

builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductGroupService, ProductGroupService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IUnitMeasureService, UnitMeasureService>();
builder.Services.AddScoped<IProductTypeService, ProductTypeService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddValidatorsFromAssemblyContaining<ProductRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<VendorRequestDTOValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CurrencyRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ProductPriceRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<OrderRequestDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<OrderProductRequestDtoValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (archMode == "Redis")
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var cacheService = services.GetRequiredService<IProductCacheService>();
            
            Console.WriteLine("Redis cache filling begins...");
            
            await cacheService.CacheAllProductsAsync();
            
            Console.WriteLine("Redis cache successfully filled!");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error populating Redis cache.");
        }
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();