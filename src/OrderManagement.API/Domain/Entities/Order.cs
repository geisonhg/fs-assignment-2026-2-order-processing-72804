using Shared.Contracts.Enums;

namespace OrderManagement.API.Domain.Entities;

public class Order
{
    public int OrderId { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Submitted;
    public decimal TotalAmount { get; set; }
    public string ShippingName { get; set; } = string.Empty;
    public string ShippingLine1 { get; set; } = string.Empty;
    public string? ShippingLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public bool GiftWrap { get; set; }
    public string? StripeSessionId { get; set; }
    public string? PaymentIntentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> Items { get; set; } = [];
    public InventoryRecord? InventoryRecord { get; set; }
    public PaymentRecord? PaymentRecord { get; set; }
    public ShipmentRecord? ShipmentRecord { get; set; }
}
