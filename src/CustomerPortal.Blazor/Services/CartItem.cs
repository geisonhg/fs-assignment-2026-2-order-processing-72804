namespace CustomerPortal.Blazor.Services;

public class CartItem
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = "";
    public decimal UnitPrice { get; init; }
    public int Quantity { get; set; }
}
