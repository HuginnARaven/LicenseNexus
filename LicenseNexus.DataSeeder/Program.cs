using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using LicenseNexus.Infrastructure.Data.Contexts;
using LicenseNexus.DataSeeder;
using LicenseNexus.DataSeeder.Seeders;

Console.WriteLine("Starting Data Seeder...");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../LicenseNexus.API"))
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);

// Using connection string names from API/Program.cs
services.AddDbContext<BaseSqlContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("MongoArchDatabase")));

services.AddDbContext<ExtendedSqlContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("RedisArchDatabase")));

services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));
services.AddScoped<MongoContext>();
services.AddSingleton<RedisContext>();

services.AddTransient<ProductSeeder>();

services.AddTransient<DatabaseSeeder>();

var serviceProvider = services.BuildServiceProvider();

var seeder = serviceProvider.GetRequiredService<DatabaseSeeder>();
await seeder.RunAsync();

Console.WriteLine("Data generation completed successfully.");