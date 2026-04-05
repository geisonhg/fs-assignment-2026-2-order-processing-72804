using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using Shared.Contracts.DTOs;
using Shared.Contracts.Enums;

namespace OrderManagement.API.CQRS.Queries;

public record GetOrdersQuery(OrderStatus? Status = null) : IRequest<List<OrderSummaryDto>>;

public class GetOrdersHandler(OrderDbContext db, IMapper mapper)
    : IRequestHandler<GetOrdersQuery, List<OrderSummaryDto>>
{
    public async Task<List<OrderSummaryDto>> Handle(GetOrdersQuery query, CancellationToken ct)
    {
        var orders = db.Orders.Include(o => o.Customer).AsQueryable();

        if (query.Status.HasValue)
            orders = orders.Where(o => o.Status == query.Status.Value);

        var result = await orders.OrderByDescending(o => o.CreatedAt).ToListAsync(ct);
        return mapper.Map<List<OrderSummaryDto>>(result);
    }
}
