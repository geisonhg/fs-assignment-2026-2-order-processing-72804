namespace OrderManagement.API.Domain.Entities;

public class PaymentRecord
{
    public int PaymentRecordId { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public bool Success { get; set; }
    public string? TransactionReference { get; set; }
    public string? FailureReason { get; set; }
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
