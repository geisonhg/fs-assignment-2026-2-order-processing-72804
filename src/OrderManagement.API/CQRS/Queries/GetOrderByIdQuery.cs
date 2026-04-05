using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using Shared.Contracts.DTOs;

namespace OrderManagement.API.CQRS.Queries;

public record GetOrderByIdQuery(int OrderId) : IRequest<OrderDto?>;

public class GetOrderByIdHandler(OrderDbContext db, IMapper mapper)
    : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderByIdQuery query, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Include(o => o.InventoryRecord)
            .Include(o => o.PaymentRecord)
            .Include(o => o.ShipmentRecord)
            .FirstOrDefaultAsync(o => o.OrderId == query.OrderId, ct);

        return order is null ? null : mapper.Map<OrderDto>(order);
    }
}
