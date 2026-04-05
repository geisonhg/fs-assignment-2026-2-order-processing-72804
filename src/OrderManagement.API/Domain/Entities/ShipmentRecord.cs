namespace OrderManagement.API.Domain.Entities;

public class ShipmentRecord
{
    public int ShipmentRecordId { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public bool Success { get; set; }
    public string? TrackingReference { get; set; }
    public DateTime? EstimatedDispatchDate { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
