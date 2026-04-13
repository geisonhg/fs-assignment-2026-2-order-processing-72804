using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using OrderManagement.API.Domain.Entities;
using OrderManagement.API.Messaging;
using Shared.Contracts.DTOs;
using Shared.Contracts.Enums;
using Shared.Contracts.Messages;

namespace OrderManagement.API.CQRS.Commands;

public record CheckoutOrderCommand(CheckoutRequestDto Request) : IRequest<OrderDto>;

public class CheckoutOrderHandler(
    OrderDbContext db,
    IRabbitMqPublisher publisher,
    IMapper mapper,
    ILogger<CheckoutOrderHandler> logger)
    : IRequestHandler<CheckoutOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CheckoutOrderCommand command, CancellationToken ct)
    {
        var req        = command.Request;
        var productIds = req.Items.Select(i => i.ProductId).ToList();
        var products   = await db.Products.Where(p => productIds.Contains(p.ProductId)).ToListAsync(ct);

        if (products.Count != productIds.Count)
            throw new InvalidOperationException("One or more products were not found.");

        var customer = await db.Customers.FindAsync([req.CustomerId], ct)
            ?? await db.Customers.FirstAsync(ct);

        var correlationId = Guid.NewGuid().ToString();
        var reference     = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var order = new Order
        {
            OrderReference  = reference,
            CorrelationId   = correlationId,
            CustomerId      = customer.CustomerId,
            Status          = OrderStatus.Submitted,
            ShippingName    = req.ShippingName,
            ShippingLine1   = req.ShippingLine1,
            ShippingLine2   = req.ShippingLine2,
            ShippingCity    = req.ShippingCity,
            ShippingCountry = req.ShippingCountry,
            GiftWrap        = req.GiftWrap,
            StripeSessionId = req.StripeSessionId,
            PaymentIntentId = req.PaymentIntentId,
            Items = req.Items.Select(i => new OrderItem
            {
                ProductId   = i.ProductId,
                ProductName = i.ProductName,
                Quantity    = i.Quantity,
                UnitPrice   = i.UnitPrice
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        var message = new OrderSubmitted
        {
            CorrelationId = correlationId,
            OrderId       = order.OrderId,
            CustomerId    = customer.CustomerId,
            TotalAmount     = order.TotalAmount,
            StripeSessionId = order.StripeSessionId,
            Items = req.Items.Select(i => new OrderItemMessage
            {
                ProductId   = i.ProductId,
                ProductName = i.ProductName,
                Quantity    = i.Quantity,
                UnitPrice   = i.UnitPrice
            }).ToList()
        };

        publisher.Publish(QueueNames.OrderSubmitted, message);

        order.Status    = OrderStatus.InventoryPending;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        using (Serilog.Context.LogContext.PushProperty("OrderId",       order.OrderId))
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        using (Serilog.Context.LogContext.PushProperty("CustomerId",    customer.CustomerId))
        {
            logger.LogInformation("Order {OrderReference} submitted. Total: {Total:C}", reference, order.TotalAmount);
        }

        var saved = await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .FirstAsync(o => o.OrderId == order.OrderId, ct);

        return mapper.Map<OrderDto>(saved);
    }
}
