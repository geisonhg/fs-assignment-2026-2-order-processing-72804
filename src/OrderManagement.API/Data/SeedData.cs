using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Domain.Entities;
using Shared.Contracts.Enums;

namespace OrderManagement.API.Data;

public static class SeedData
{
    public static void Initialize(IServiceProvider services)
    {
        using var scope   = services.CreateScope();
        var context       = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var logger        = scope.ServiceProvider.GetRequiredService<ILogger<OrderDbContext>>();

        context.Database.Migrate();

        if (context.Products.Any())
        {
            logger.LogInformation("Database already seeded — skipping");
            return;
        }

        logger.LogInformation("Seeding database...");

        var products = new List<Product>
        {
            new() { Name = "Kayak",             Description = "A boat for one person",                Price = 275m,   Category = "Watersports", StockQuantity = 10 },
            new() { Name = "Lifejacket",        Description = "Protective and fashionable",           Price = 48.95m, Category = "Watersports", StockQuantity = 25 },
            new() { Name = "Soccer Ball",       Description = "FIFA-approved size and weight",        Price = 19.5m,  Category = "Soccer",      StockQuantity = 50 },
            new() { Name = "Corner Flags",      Description = "Give your pitch a professional look",  Price = 34.95m, Category = "Soccer",      StockQuantity = 0  },
            new() { Name = "Stadium",           Description = "Flat-packed 35,000-seat stadium",      Price = 79500m, Category = "Soccer",      StockQuantity = 3  },
            new() { Name = "Thinking Cap",      Description = "Improve brain efficiency by 75%",      Price = 16m,    Category = "Chess",       StockQuantity = 30 },
            new() { Name = "Unsteady Chair",    Description = "Secretly give your opponent back pain",Price = 29.95m, Category = "Chess",       StockQuantity = 15 },
            new() { Name = "Human Chess Board", Description = "A fun game for the family",            Price = 75m,    Category = "Chess",       StockQuantity = 8  },
            new() { Name = "Bling-Bling King",  Description = "Gold-plated, diamond-studded King",   Price = 1200m,  Category = "Chess",       StockQuantity = 2  },
        };
        context.Products.AddRange(products);

        var demoCustomer = new Customer { Name = "Demo Customer", Email = "demo@sportsstore.com", Phone = "0851234567" };
        var adminCustomer = new Customer { Name = "Admin User", Email = "admin@sportsstore.com" };
        context.Customers.AddRange(demoCustomer, adminCustomer);
        context.SaveChanges();

        var sampleOrders = new List<Order>
        {
            new()
            {
                OrderReference  = "ORD-20260401-DEMO01",
                CorrelationId   = Guid.NewGuid().ToString(),
                CustomerId      = demoCustomer.CustomerId,
                Status          = OrderStatus.Completed,
                TotalAmount     = 294.5m,
                ShippingName    = "Demo Customer",
                ShippingLine1   = "1 Main Street",
                ShippingCity    = "Dublin",
                ShippingCountry = "Ireland",
                CreatedAt       = DateTime.UtcNow.AddDays(-3),
                UpdatedAt       = DateTime.UtcNow.AddDays(-3),
                Items =
                [
                    new() { ProductId = 1, ProductName = "Kayak",      Quantity = 1, UnitPrice = 275m  },
                    new() { ProductId = 3, ProductName = "Soccer Ball", Quantity = 1, UnitPrice = 19.5m },
                ]
            },
            new()
            {
                OrderReference  = "ORD-20260402-DEMO02",
                CorrelationId   = Guid.NewGuid().ToString(),
                CustomerId      = demoCustomer.CustomerId,
                Status          = OrderStatus.PaymentFailed,
                TotalAmount     = 48.95m,
                ShippingName    = "Demo Customer",
                ShippingLine1   = "1 Main Street",
                ShippingCity    = "Dublin",
                ShippingCountry = "Ireland",
                CreatedAt       = DateTime.UtcNow.AddDays(-1),
                UpdatedAt       = DateTime.UtcNow.AddDays(-1),
                Items =
                [
                    new() { ProductId = 2, ProductName = "Lifejacket", Quantity = 1, UnitPrice = 48.95m }
                ]
            },
        };
        context.Orders.AddRange(sampleOrders);
        context.SaveChanges();

        logger.LogInformation("Database seeded with {ProductCount} products and {OrderCount} sample orders",
            products.Count, sampleOrders.Count);
    }
}
