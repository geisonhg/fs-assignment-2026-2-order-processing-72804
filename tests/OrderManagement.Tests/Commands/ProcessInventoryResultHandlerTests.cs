using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderManagement.API.CQRS.Commands;
using OrderManagement.API.Data;
using OrderManagement.API.Domain.Entities;
using OrderManagement.API.Messaging;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;

namespace OrderManagement.Tests.Commands;

public class ProcessInventoryResultHandlerTests
{
    private static OrderDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new OrderDbContext(options);
    }

    [Fact]
    public async Task InventoryConfirmed_UpdatesStatusToPaymentPending_AndPublishesPaymentRequest()
    {
        var db        = CreateInMemoryDb();
        var publisher = new Mock<IRabbitMqPublisher>();
        var logger    = NullLogger<ProcessInventoryResultHandler>.Instance;

        var order = new Order
        {
            OrderReference = "ORD-TEST", CorrelationId = Guid.NewGuid().ToString(),
            CustomerId = 1, Status = OrderStatus.InventoryPending, TotalAmount = 100m
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var message = new InventoryCheckCompleted
        {
            CorrelationId = order.CorrelationId,
            OrderId       = order.OrderId,
            Success       = true
        };

        var handler = new ProcessInventoryResultHandler(db, publisher.Object, logger);
        await handler.Handle(new ProcessInventoryResultCommand(message), CancellationToken.None);

        var updated = await db.Orders.FirstAsync();
        updated.Status.Should().Be(OrderStatus.PaymentPending);

        publisher.Verify(p => p.Publish(
            It.Is<string>(q => q == "payment-requested"),
            It.IsAny<object>()), Times.Once);

        var record = await db.InventoryRecords.FirstAsync();
        record.Success.Should().BeTrue();
    }

    [Fact]
    public async Task InventoryFailed_UpdatesStatusToFailed_DoesNotPublishPaymentRequest()
    {
        var db        = CreateInMemoryDb();
        var publisher = new Mock<IRabbitMqPublisher>();
        var logger    = NullLogger<ProcessInventoryResultHandler>.Instance;

        var order = new Order
        {
            OrderReference = "ORD-TEST", CorrelationId = Guid.NewGuid().ToString(),
            CustomerId = 1, Status = OrderStatus.InventoryPending, TotalAmount = 100m
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var message = new InventoryCheckCompleted
        {
            CorrelationId = order.CorrelationId,
            OrderId       = order.OrderId,
            Success       = false,
            FailureReason = "Out of stock"
        };

        var handler = new ProcessInventoryResultHandler(db, publisher.Object, logger);
        await handler.Handle(new ProcessInventoryResultCommand(message), CancellationToken.None);

        var updated = await db.Orders.FirstAsync();
        updated.Status.Should().Be(OrderStatus.Failed);

        publisher.Verify(p => p.Publish(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }
}
