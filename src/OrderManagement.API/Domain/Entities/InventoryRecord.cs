namespace OrderManagement.API.Domain.Entities;

public class InventoryRecord
{
    public int InventoryRecordId { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
