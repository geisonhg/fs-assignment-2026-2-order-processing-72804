using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using Shared.Contracts.DTOs;

namespace OrderManagement.API.CQRS.Queries;

public record GetCustomerOrdersQuery(int CustomerId) : IRequest<List<OrderSummaryDto>>;

public class GetCustomerOrdersHandler(OrderDbContext db, IMapper mapper)
    : IRequestHandler<GetCustomerOrdersQuery, List<OrderSummaryDto>>
{
    public async Task<List<OrderSummaryDto>> Handle(GetCustomerOrdersQuery query, CancellationToken ct)
    {
        var orders = await db.Orders
            .Include(o => o.Customer)
            .Where(o => o.CustomerId == query.CustomerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        return mapper.Map<List<OrderSummaryDto>>(orders);
    }
}
