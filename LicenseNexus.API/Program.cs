using LicenseNexus.Domain.Interfaces;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.Infrastructure.Repositories;
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


var archMode = builder.Configuration["ArchitectureMode"];
if (archMode == "Redis")
{
    builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<ExtendedSqlContext>());
    builder.Services.AddScoped<ICartRepository, RedisCartRepository>();
    builder.Services.AddScoped<IProductRepository, RedisProductRepository>();
}
else // Mongo
{
    builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<BaseSqlContext>());
    builder.Services.AddScoped<BaseSqlContext>(provider => provider.GetRequiredService<ExtendedSqlContext>());
    builder.Services.AddScoped<ICartRepository, MongoCartRepository>();
    builder.Services.AddScoped<IProductRepository, MongoProductRepository>();
}

builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();