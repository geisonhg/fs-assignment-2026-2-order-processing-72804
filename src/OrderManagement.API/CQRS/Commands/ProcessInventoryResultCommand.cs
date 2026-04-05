using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using OrderManagement.API.Domain.Entities;
using OrderManagement.API.Messaging;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;

namespace OrderManagement.API.CQRS.Commands;

public record ProcessInventoryResultCommand(InventoryCheckCompleted Message) : IRequest;

public class ProcessInventoryResultHandler(
    OrderDbContext db,
    IRabbitMqPublisher publisher,
    ILogger<ProcessInventoryResultHandler> logger)
    : IRequestHandler<ProcessInventoryResultCommand>
{
    public async Task Handle(ProcessInventoryResultCommand command, CancellationToken ct)
    {
        var msg   = command.Message;
        var order = await db.Orders.FindAsync([msg.OrderId], ct);

        if (order is null)
        {
            logger.LogWarning("ProcessInventoryResult: Order {OrderId} not found", msg.OrderId);
            return;
        }

        db.InventoryRecords.Add(new InventoryRecord
        {
            OrderId       = order.OrderId,
            Success       = msg.Success,
            FailureReason = msg.FailureReason,
            ProcessedAt   = msg.ProcessedAt
        });

        if (msg.Success)
        {
            order.Status = OrderStatus.InventoryConfirmed;

            publisher.Publish(QueueNames.PaymentRequested, new OrderSubmitted
            {
                CorrelationId = msg.CorrelationId,
                OrderId       = order.OrderId,
                CustomerId    = order.CustomerId,
                TotalAmount   = order.TotalAmount,
                Items         = []
            });

            order.Status = OrderStatus.PaymentPending;
            logger.LogInformation("Inventory confirmed for Order {OrderId}. Payment request sent", order.OrderId);
        }
        else
        {
            order.Status = OrderStatus.Failed;
            logger.LogWarning("Inventory failed for Order {OrderId}: {Reason}", order.OrderId, msg.FailureReason);
        }

        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
