using Microsoft.EntityFrameworkCore;

namespace Inventory.Service.Data;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<InventoryProduct> Products => Set<InventoryProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryProduct>()
            .ToTable("Products")
            .HasKey(p => p.ProductId);
    }
}

public class InventoryProduct
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}
