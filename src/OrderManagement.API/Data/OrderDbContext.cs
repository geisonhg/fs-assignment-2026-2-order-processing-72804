using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Domain.Entities;

namespace OrderManagement.API.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Product>         Products         => Set<Product>();
    public DbSet<Customer>        Customers        => Set<Customer>();
    public DbSet<Order>           Orders           => Set<Order>();
    public DbSet<OrderItem>       OrderItems       => Set<OrderItem>();
    public DbSet<InventoryRecord> InventoryRecords => Set<InventoryRecord>();
    public DbSet<PaymentRecord>   PaymentRecords   => Set<PaymentRecord>();
    public DbSet<ShipmentRecord>  ShipmentRecords  => Set<ShipmentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(8,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.UnitPrice)
            .HasColumnType("decimal(8,2)");

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<PaymentRecord>()
            .Property(p => p.Amount)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .HasOne(o => o.InventoryRecord)
            .WithOne(i => i.Order)
            .HasForeignKey<InventoryRecord>(i => i.OrderId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.PaymentRecord)
            .WithOne(p => p.Order)
            .HasForeignKey<PaymentRecord>(p => p.OrderId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.ShipmentRecord)
            .WithOne(s => s.Order)
            .HasForeignKey<ShipmentRecord>(s => s.OrderId);
    }
}
