using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Payment.Service.Services;
using Shared.Contracts.Messages;

namespace OrderManagement.Tests.Services;

public class PaymentProcessorTests
{
    [Fact]
    public void Process_ValidOrder_ReturnsPaymentResult()
    {
        var logger    = NullLogger<PaymentProcessor>.Instance;
        var processor = new PaymentProcessor(logger);

        var order = new OrderSubmitted
        {
            CorrelationId = Guid.NewGuid().ToString(),
            OrderId       = 1,
            CustomerId    = 1,
            TotalAmount   = 150m,
            Items         = []
        };

        var result = processor.Process(order);

        result.Should().NotBeNull();
        result.OrderId.Should().Be(1);
        result.Amount.Should().Be(150m);
        result.CorrelationId.Should().Be(order.CorrelationId);
    }

    [Fact]
    public void Process_ApprovedPayment_HasTransactionReference()
    {
        var logger    = NullLogger<PaymentProcessor>.Instance;
        var processor = new PaymentProcessor(logger);

        var order = new OrderSubmitted
        {
            CorrelationId = Guid.NewGuid().ToString(),
            OrderId       = 1,
            CustomerId    = 1,
            TotalAmount   = 50m,
            Items         = []
        };

        // Run multiple times to hit an approval (90% chance)
        // This is a probabilistic test — statistical certainty with enough runs
        var results = Enumerable.Range(0, 20)
            .Select(_ => processor.Process(order))
            .ToList();

        results.Should().Contain(r => r.Success && r.TransactionReference!.StartsWith("PAY-"));
    }

    [Fact]
    public void Process_RejectedPayment_HasFailureReason()
    {
        var logger    = NullLogger<PaymentProcessor>.Instance;
        var processor = new PaymentProcessor(logger);

        var order = new OrderSubmitted
        {
            CorrelationId = Guid.NewGuid().ToString(),
            OrderId       = 1,
            CustomerId    = 1,
            TotalAmount   = 50m,
            Items         = []
        };

        // Run multiple times to hit a rejection (10% chance)
        var results = Enumerable.Range(0, 50)
            .Select(_ => processor.Process(order))
            .ToList();

        results.Should().Contain(r => !r.Success && r.FailureReason != null);
    }
}
