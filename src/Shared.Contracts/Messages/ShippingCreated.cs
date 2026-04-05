namespace Shared.Contracts.Messages;

public class ShippingCreated
{
    public required string CorrelationId { get; init; }
    public required int OrderId { get; init; }
    public required bool Success { get; init; }
    public string? TrackingReference { get; init; }
    public DateTime? EstimatedDispatchDate { get; init; }
    public string? FailureReason { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
