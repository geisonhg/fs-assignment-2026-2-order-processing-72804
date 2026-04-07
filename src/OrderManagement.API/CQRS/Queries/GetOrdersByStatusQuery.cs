using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using Shared.Contracts.DTOs;
using Shared.Contracts.Enums;

namespace OrderManagement.API.CQRS.Queries;

public record GetOrdersByStatusQuery(OrderStatus Status) : IRequest<List<OrderSummaryDto>>;

public class GetOrdersByStatusHandler(OrderDbContext db, IMapper mapper)
    : IRequestHandler<GetOrdersByStatusQuery, List<OrderSummaryDto>>
{
    public async Task<List<OrderSummaryDto>> Handle(GetOrdersByStatusQuery query, CancellationToken ct)
    {
        var orders = await db.Orders
            .Include(o => o.Customer)
            .Where(o => o.Status == query.Status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        return mapper.Map<List<OrderSummaryDto>>(orders);
    }
}
