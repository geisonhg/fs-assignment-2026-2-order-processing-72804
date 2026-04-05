namespace Shared.Contracts.Messages;

public class InventoryCheckCompleted
{
    public required string CorrelationId { get; init; }
    public required int OrderId { get; init; }
    public required bool Success { get; init; }
    public string? FailureReason { get; init; }
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
}
