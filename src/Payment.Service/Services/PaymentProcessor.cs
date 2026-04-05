using Shared.Contracts.Messages;

namespace Payment.Service.Services;

public class PaymentProcessor(ILogger<PaymentProcessor> logger)
{
    private static readonly Random _random = new();

    public PaymentProcessed Process(OrderSubmitted order)
    {
        Thread.Sleep(500);
        var approved  = _random.Next(1, 11) <= 9;
        var reference = $"PAY-{Guid.NewGuid().ToString("N")[..10].ToUpper()}";

        if (approved)
        {
            logger.LogInformation("Payment APPROVED for OrderId={OrderId}. Amount={Amount:C}. Ref={Reference}",
                order.OrderId, order.TotalAmount, reference);
            return new PaymentProcessed
            {
                CorrelationId        = order.CorrelationId,
                OrderId              = order.OrderId,
                Success              = true,
                TransactionReference = reference,
                Amount               = order.TotalAmount
            };
        }

        logger.LogWarning("Payment REJECTED for OrderId={OrderId}. Amount={Amount:C}", order.OrderId, order.TotalAmount);
        return new PaymentProcessed
        {
            CorrelationId = order.CorrelationId,
            OrderId       = order.OrderId,
            Success       = false,
            FailureReason = "Payment declined by processor.",
            Amount        = order.TotalAmount
        };
    }
}
