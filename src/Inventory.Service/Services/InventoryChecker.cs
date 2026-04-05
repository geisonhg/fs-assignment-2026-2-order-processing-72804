using Inventory.Service.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Messages;

namespace Inventory.Service.Services;

public class InventoryChecker(InventoryDbContext db, ILogger<InventoryChecker> logger)
{
    public async Task<InventoryCheckCompleted> CheckAsync(OrderSubmitted order)
    {
        var productIds = order.Items.Select(i => i.ProductId).ToList();
        var stocks     = await db.Products
            .Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId, p => p);

        foreach (var item in order.Items)
        {
            if (!stocks.TryGetValue(item.ProductId, out var product))
            {
                logger.LogWarning("InventoryCheck: ProductId {ProductId} not found. OrderId={OrderId}",
                    item.ProductId, order.OrderId);
                return Fail(order, $"Product '{item.ProductName}' not found in inventory.");
            }

            if (product.StockQuantity < item.Quantity)
            {
                logger.LogWarning(
                    "InventoryCheck: Insufficient stock for {Product}. Requested={Requested}, Available={Available}. OrderId={OrderId}",
                    product.Name, item.Quantity, product.StockQuantity, order.OrderId);
                return Fail(order,
                    $"Insufficient stock for '{product.Name}'. Requested: {item.Quantity}, Available: {product.StockQuantity}.");
            }
        }

        logger.LogInformation("InventoryCheck: All {ItemCount} items confirmed for OrderId={OrderId}",
            order.Items.Count, order.OrderId);

        return new InventoryCheckCompleted
        {
            CorrelationId = order.CorrelationId,
            OrderId       = order.OrderId,
            Success       = true
        };
    }

    private static InventoryCheckCompleted Fail(OrderSubmitted order, string reason) =>
        new() { CorrelationId = order.CorrelationId, OrderId = order.OrderId, Success = false, FailureReason = reason };
}
