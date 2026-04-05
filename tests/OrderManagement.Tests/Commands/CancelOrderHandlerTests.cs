using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderManagement.API.CQRS.Commands;
using OrderManagement.API.Data;
using OrderManagement.API.Domain.Entities;
using Shared.Contracts.Enums;

namespace OrderManagement.Tests.Commands;

public class CancelOrderHandlerTests
{
    private static OrderDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new OrderDbContext(options);
    }

    [Fact]
    public async Task Cancel_EarlyStageOrder_ReturnsTrue()
    {
        var db     = CreateInMemoryDb();
        var logger = NullLogger<CancelOrderHandler>.Instance;

        var order = new Order
        {
            OrderReference = "ORD-TEST", CorrelationId = "c1",
            CustomerId = 1, Status = OrderStatus.InventoryPending, TotalAmount = 100m
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var handler = new CancelOrderHandler(db, logger);
        var result  = await handler.Handle(new CancelOrderCommand(order.OrderId), CancellationToken.None);

        result.Should().BeTrue();
        var updated = await db.Orders.FirstAsync();
        updated.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Cancel_CompletedOrder_ReturnsFalse()
    {
        var db     = CreateInMemoryDb();
        var logger = NullLogger<CancelOrderHandler>.Instance;

        var order = new Order
        {
            OrderReference = "ORD-TEST", CorrelationId = "c1",
            CustomerId = 1, Status = OrderStatus.Completed, TotalAmount = 100m
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var handler = new CancelOrderHandler(db, logger);
        var result  = await handler.Handle(new CancelOrderCommand(order.OrderId), CancellationToken.None);

        result.Should().BeFalse();
        var updated = await db.Orders.FirstAsync();
        updated.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Cancel_NonExistentOrder_ReturnsFalse()
    {
        var db     = CreateInMemoryDb();
        var logger = NullLogger<CancelOrderHandler>.Instance;

        var handler = new CancelOrderHandler(db, logger);
        var result  = await handler.Handle(new CancelOrderCommand(999), CancellationToken.None);

        result.Should().BeFalse();
    }
}
