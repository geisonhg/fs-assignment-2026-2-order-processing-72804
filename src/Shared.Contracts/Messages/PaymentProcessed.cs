namespace Shared.Contracts.Messages;

public class PaymentProcessed
{
    public required string CorrelationId { get; init; }
    public required int OrderId { get; init; }
    public required bool Success { get; init; }
    public string? TransactionReference { get; init; }
    public string? FailureReason { get; init; }
    public decimal Amount { get; init; }
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
}
