using Shared.Contracts.Enums;

namespace Shared.Contracts.DTOs;

public class OrderDto
{
    public int OrderId { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string ShippingName { get; set; } = string.Empty;
    public string ShippingLine1 { get; set; } = string.Empty;
    public string? ShippingLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public bool GiftWrap { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
    public InventoryResultDto? InventoryResult { get; set; }
    public PaymentResultDto? PaymentResult { get; set; }
    public ShipmentResultDto? ShipmentResult { get; set; }
}

public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => UnitPrice * Quantity;
}

public class InventoryResultDto
{
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class PaymentResultDto
{
    public bool Success { get; set; }
    public string? TransactionReference { get; set; }
    public string? FailureReason { get; set; }
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class ShipmentResultDto
{
    public bool Success { get; set; }
    public string? TrackingReference { get; set; }
    public DateTime? EstimatedDispatchDate { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
