namespace Shared.Contracts.Messages;

public class OrderSubmitted
{
    public required string CorrelationId { get; init; }
    public required int OrderId { get; init; }
    public required int CustomerId { get; init; }
    public required List<OrderItemMessage> Items { get; init; }
    public required decimal TotalAmount { get; init; }
    public DateTime SubmittedAt { get; init; } = DateTime.UtcNow;
}

public class OrderItemMessage
{
    public required int ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}
