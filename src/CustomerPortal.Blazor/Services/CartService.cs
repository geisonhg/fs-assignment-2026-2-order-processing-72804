namespace CustomerPortal.Blazor.Services;

public class CartService
{
    private readonly List<CartItem> _items = [];

    public IReadOnlyList<CartItem> Items => _items;
    public int TotalItems => _items.Sum(i => i.Quantity);
    public decimal Total => _items.Sum(i => i.UnitPrice * i.Quantity);

    public void Add(int productId, string name, decimal price)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
            existing.Quantity++;
        else
            _items.Add(new CartItem { ProductId = productId, ProductName = name, UnitPrice = price, Quantity = 1 });
    }

    public void Increment(int productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null) item.Quantity++;
    }

    public void Decrement(int productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return;
        if (item.Quantity <= 1) _items.Remove(item);
        else item.Quantity--;
    }

    public void Remove(int productId) =>
        _items.RemoveAll(i => i.ProductId == productId);

    public void Clear() => _items.Clear();
}
