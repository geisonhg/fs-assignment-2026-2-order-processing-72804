using MediatR;
using OrderManagement.API.Data;
using Shared.Contracts.Enums;

namespace OrderManagement.API.CQRS.Commands;

public record CancelOrderCommand(int OrderId) : IRequest<bool>;

public class CancelOrderHandler(OrderDbContext db, ILogger<CancelOrderHandler> logger)
    : IRequestHandler<CancelOrderCommand, bool>
{
    public async Task<bool> Handle(CancelOrderCommand command, CancellationToken ct)
    {
        var order = await db.Orders.FindAsync([command.OrderId], ct);
        if (order is null) return false;

        var cancellable = new[] { OrderStatus.Submitted, OrderStatus.InventoryPending };
        if (!cancellable.Contains(order.Status))
        {
            logger.LogWarning("Cannot cancel Order {OrderId} — status is {Status}", order.OrderId, order.Status);
            return false;
        }

        order.Status    = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} cancelled", order.OrderId);
        return true;
    }
}
