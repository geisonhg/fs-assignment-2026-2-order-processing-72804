using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using OrderManagement.API.Domain.Entities;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;

namespace OrderManagement.API.CQRS.Commands;

public record ProcessShippingResultCommand(ShippingCreated Message) : IRequest;

public class ProcessShippingResultHandler(
    OrderDbContext db,
    ILogger<ProcessShippingResultHandler> logger)
    : IRequestHandler<ProcessShippingResultCommand>
{
    public async Task Handle(ProcessShippingResultCommand command, CancellationToken ct)
    {
        var msg   = command.Message;
        var order = await db.Orders.FindAsync([msg.OrderId], ct);

        if (order is null)
        {
            logger.LogWarning("ProcessShippingResult: Order {OrderId} not found", msg.OrderId);
            return;
        }

        db.ShipmentRecords.Add(new ShipmentRecord
        {
            OrderId               = order.OrderId,
            Success               = msg.Success,
            TrackingReference     = msg.TrackingReference,
            EstimatedDispatchDate = msg.EstimatedDispatchDate,
            FailureReason         = msg.FailureReason,
            CreatedAt             = msg.CreatedAt
        });

        order.Status    = msg.Success ? OrderStatus.Completed : OrderStatus.Failed;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} is now {Status}. Tracking: {Tracking}",
            order.OrderId, order.Status, msg.TrackingReference);
    }
}
