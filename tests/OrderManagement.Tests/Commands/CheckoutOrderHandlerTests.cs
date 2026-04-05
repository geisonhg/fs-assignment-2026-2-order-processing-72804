using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderManagement.API.CQRS.Commands;
using OrderManagement.API.Data;
using OrderManagement.API.Domain.Entities;
using OrderManagement.API.Mapping;
using OrderManagement.API.Messaging;
using Shared.Contracts.DTOs;
using Shared.Contracts.Enums;

namespace OrderManagement.Tests.Commands;

public class CheckoutOrderHandlerTests
{
    private static OrderDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new OrderDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task Checkout_ValidRequest_CreatesOrderAndPublishesEvent()
    {
        // Arrange
        var db        = CreateInMemoryDb();
        var publisher = new Mock<IRabbitMqPublisher>();
        var mapper    = CreateMapper();
        var logger    = NullLogger<CheckoutOrderHandler>.Instance;

        var customer = new Customer { Name = "Test Customer", Email = "test@test.com" };
        var product  = new Product  { Name = "Kayak", Price = 275m, StockQuantity = 10, Category = "Watersports", Description = "A boat" };
        db.Customers.Add(customer);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var request = new CheckoutRequestDto
        {
            CustomerId      = customer.CustomerId,
            ShippingName    = "Test Customer",
            ShippingLine1   = "1 Main St",
            ShippingCity    = "Dublin",
            ShippingCountry = "Ireland",
            Items =
            [
                new CartItemDto { ProductId = product.ProductId, ProductName = "Kayak", Quantity = 1, UnitPrice = 275m }
            ]
        };

        var handler = new CheckoutOrderHandler(db, publisher.Object, mapper, logger);

        // Act
        var result = await handler.Handle(new CheckoutOrderCommand(request), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(275m);
        result.Status.Should().Be(OrderStatus.InventoryPending);
        result.Items.Should().HaveCount(1);

        // Verify RabbitMQ event was published
        publisher.Verify(p => p.Publish(
            It.Is<string>(q => q == "order-submitted"),
            It.IsAny<object>()), Times.Once);

        // Verify order was persisted
        var savedOrder = await db.Orders.FirstAsync();
        savedOrder.OrderReference.Should().StartWith("ORD-");
        savedOrder.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Checkout_InvalidProduct_ThrowsException()
    {
        // Arrange
        var db        = CreateInMemoryDb();
        var publisher = new Mock<IRabbitMqPublisher>();
        var mapper    = CreateMapper();
        var logger    = NullLogger<CheckoutOrderHandler>.Instance;

        var customer = new Customer { Name = "Test", Email = "test@test.com" };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var request = new CheckoutRequestDto
        {
            CustomerId = customer.CustomerId,
            Items      = [ new CartItemDto { ProductId = 999, ProductName = "Ghost Product", Quantity = 1, UnitPrice = 10m } ]
        };

        var handler = new CheckoutOrderHandler(db, publisher.Object, mapper, logger);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CheckoutOrderCommand(request), CancellationToken.None));

        publisher.Verify(p => p.Publish(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Checkout_CalculatesTotalCorrectly()
    {
        var db        = CreateInMemoryDb();
        var publisher = new Mock<IRabbitMqPublisher>();
        var mapper    = CreateMapper();
        var logger    = NullLogger<CheckoutOrderHandler>.Instance;

        var customer = new Customer { Name = "Test", Email = "test@test.com" };
        var p1 = new Product { Name = "Ball",    Price = 20m,  StockQuantity = 10, Category = "Soccer",     Description = "Ball" };
        var p2 = new Product { Name = "Lifejacket", Price = 50m, StockQuantity = 10, Category = "Watersports", Description = "LJ" };
        db.Customers.Add(customer);
        db.Products.AddRange(p1, p2);
        await db.SaveChangesAsync();

        var request = new CheckoutRequestDto
        {
            CustomerId      = customer.CustomerId,
            ShippingName    = "Test", ShippingLine1 = "1 St", ShippingCity = "Dublin", ShippingCountry = "IE",
            Items =
            [
                new CartItemDto { ProductId = p1.ProductId, ProductName = "Ball",       Quantity = 3, UnitPrice = 20m },
                new CartItemDto { ProductId = p2.ProductId, ProductName = "Lifejacket", Quantity = 2, UnitPrice = 50m },
            ]
        };

        var handler = new CheckoutOrderHandler(db, publisher.Object, mapper, logger);
        var result  = await handler.Handle(new CheckoutOrderCommand(request), CancellationToken.None);

        // 3 * 20 + 2 * 50 = 60 + 100 = 160
        result.TotalAmount.Should().Be(160m);
    }
}
