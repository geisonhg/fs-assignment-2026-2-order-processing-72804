namespace Shared.Contracts.DTOs;

public class CheckoutRequestDto
{
    public int CustomerId { get; set; }
    public List<CartItemDto> Items { get; set; } = [];
    public string ShippingName { get; set; } = string.Empty;
    public string ShippingLine1 { get; set; } = string.Empty;
    public string? ShippingLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public bool GiftWrap { get; set; }
}
