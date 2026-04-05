namespace Shared.Contracts.Messages;

public class OrderFailed
{
    public required string CorrelationId { get; init; }
    public required int OrderId { get; init; }
    public required string Reason { get; init; }
    public required string FailedAtStage { get; init; }
    public DateTime FailedAt { get; init; } = DateTime.UtcNow;
}
