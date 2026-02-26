using Microsoft.EntityFrameworkCore;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Infrastructure.Data.Contexts;

public class ExtendedSqlContext : BaseSqlContext
{
    public ExtendedSqlContext(DbContextOptions<ExtendedSqlContext> options) : base(options)
    {
    }
    
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductPrice> ProductPrices { get; set; }
    public DbSet<FullDescription> FullDescriptions { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductGroup> ProductGroups { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductType> ProductTypes { get; set; }
    public DbSet<UnitMeasure> UnitMeasures { get; set; }
    public DbSet<Currency> Currencies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product -> Prices (One-to-Many)
        modelBuilder.Entity<Product>()
            .HasMany(p => p.Prices)
            .WithOne(pp => pp.Product)
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product -> FullDescription (One-to-Many)
        modelBuilder.Entity<Product>()
            .HasMany(p => p.FullDescriptions)
            .WithOne(fd => fd.Product)
            .HasForeignKey(fd => fd.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

            // Product -> Tags (Many-to-Many)
        modelBuilder.Entity<ProductTag>()
            .HasOne(pt => pt.Product)
            .WithMany(p => p.ProductTags)
            .HasForeignKey(pt => pt.ProductId);
        modelBuilder.Entity<ProductTag>()
            .HasOne(pt => pt.Tag)
            .WithMany(t => t.ProductTags)
            .HasForeignKey(pt => pt.TagId);

        // Product -> Catalogs (Restricted Delete)
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Vendor).WithMany().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Currency).WithMany().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Product>()
            .HasOne(p => p.ProductType).WithMany().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Product>()
            .HasOne(p => p.UnitMeasure).WithMany().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Product>()
            .HasOne(p => p.ProductGroup).WithMany().OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<ProductPrice>().Property(p => p.Price).HasPrecision(18, 2);
            
        // OrderProduct -> Product 
        modelBuilder.Entity<OrderProduct>()
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(op => op.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}