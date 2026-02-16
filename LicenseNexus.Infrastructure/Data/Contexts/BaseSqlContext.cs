using Microsoft.EntityFrameworkCore;
using LicenseNexus.Domain.Entities;

namespace LicenseNexus.Infrastructure.Data.Contexts;

public class BaseSqlContext : DbContext
{
    public BaseSqlContext(DbContextOptions<BaseSqlContext> options) : base(options)
    {
    }
    
    protected BaseSqlContext(DbContextOptions options) : base(options)
    {
    }
    
    public DbSet<Partner> Partners { get; set; }
    public DbSet<PartnerAddress> PartnerAddresses { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<OrderStatus> OrderStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Partner -> Addresses
        modelBuilder.Entity<Partner>()
            .HasMany(p => p.Addresses)
            .WithOne(a => a.Partner)
            .HasForeignKey(a => a.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Partner -> Customers (One-to-Many)
        modelBuilder.Entity<Partner>()
            .HasMany(p => p.Customers)
            .WithOne(c => c.Partner)
            .HasForeignKey(c => c.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Order -> Customer (One-to-Many)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order -> OrderStatus (One-to-Many)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.OrderStatus)
            .WithMany()
            .HasForeignKey(o => o.OrderStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order -> OrderProduct (One-to-Many)
        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderProducts)
            .WithOne(op => op.Order)
            .HasForeignKey(op => op.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Order>().Property(o => o.OrderTotalSum).HasPrecision(18, 2);
        modelBuilder.Entity<OrderProduct>().Property(op => op.CustomerPrice).HasPrecision(18, 2);
        modelBuilder.Entity<OrderProduct>().Property(op => op.PartnerPrice).HasPrecision(18, 2);
        modelBuilder.Entity<OrderProduct>().Property(op => op.SumTotal).HasPrecision(18, 2);
    }
}