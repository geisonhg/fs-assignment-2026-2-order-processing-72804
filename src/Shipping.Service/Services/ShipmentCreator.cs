using Shared.Contracts.Messages;

namespace Shipping.Service.Services;

public class ShipmentCreator(ILogger<ShipmentCreator> logger)
{
    public ShippingCreated CreateShipment(OrderSubmitted order)
    {
        Thread.Sleep(300);
        var tracking     = $"SHIP-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
        var dispatchDate = DateTime.UtcNow.AddDays(3);

        logger.LogInformation("Shipment created for OrderId={OrderId}. Tracking={Tracking}, Dispatch={Date:yyyy-MM-dd}",
            order.OrderId, tracking, dispatchDate);

        return new ShippingCreated
        {
            CorrelationId         = order.CorrelationId,
            OrderId               = order.OrderId,
            Success               = true,
            TrackingReference     = tracking,
            EstimatedDispatchDate = dispatchDate
        };
    }
}
