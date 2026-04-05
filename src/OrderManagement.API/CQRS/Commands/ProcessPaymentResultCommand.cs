using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using OrderManagement.API.Domain.Entities;
using OrderManagement.API.Messaging;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;

namespace OrderManagement.API.CQRS.Commands;

public record ProcessPaymentResultCommand(PaymentProcessed Message) : IRequest;

public class ProcessPaymentResultHandler(
    OrderDbContext db,
    IRabbitMqPublisher publisher,
    ILogger<ProcessPaymentResultHandler> logger)
    : IRequestHandler<ProcessPaymentResultCommand>
{
    public async Task Handle(ProcessPaymentResultCommand command, CancellationToken ct)
    {
        var msg   = command.Message;
        var order = await db.Orders.FindAsync([msg.OrderId], ct);

        if (order is null)
        {
            logger.LogWarning("ProcessPaymentResult: Order {OrderId} not found", msg.OrderId);
            return;
        }

        db.PaymentRecords.Add(new PaymentRecord
        {
            OrderId              = order.OrderId,
            Success              = msg.Success,
            TransactionReference = msg.TransactionReference,
            FailureReason        = msg.FailureReason,
            Amount               = msg.Amount,
            ProcessedAt          = msg.ProcessedAt
        });

        if (msg.Success)
        {
            order.Status = OrderStatus.PaymentApproved;

            publisher.Publish(QueueNames.ShippingRequested, new OrderSubmitted
            {
                CorrelationId = msg.CorrelationId,
                OrderId       = order.OrderId,
                CustomerId    = order.CustomerId,
                TotalAmount   = order.TotalAmount,
                Items         = []
            });

            order.Status = OrderStatus.ShippingPending;
            logger.LogInformation("Payment approved for Order {OrderId}. Ref: {Ref}. Shipping request sent",
                order.OrderId, msg.TransactionReference);
        }
        else
        {
            order.Status = OrderStatus.Failed;
            logger.LogWarning("Payment rejected for Order {OrderId}: {Reason}", order.OrderId, msg.FailureReason);
        }

        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
