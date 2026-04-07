using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using Shared.Contracts.DTOs;
using Shared.Contracts.Enums;

namespace OrderManagement.API.CQRS.Queries;

public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;

public class GetDashboardSummaryHandler(OrderDbContext db)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private static readonly HashSet<OrderStatus> TerminalStatuses =
    [
        OrderStatus.Completed, OrderStatus.Failed, OrderStatus.Cancelled
    ];

    private static readonly HashSet<OrderStatus> PendingStatuses =
    [
        OrderStatus.Submitted,
        OrderStatus.InventoryPending,
        OrderStatus.InventoryConfirmed,
        OrderStatus.PaymentPending,
        OrderStatus.PaymentApproved,
        OrderStatus.ShippingPending,
        OrderStatus.ShippingCreated
    ];

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery _, CancellationToken ct)
    {
        var orders = await db.Orders.AsNoTracking().ToListAsync(ct);

        var byStatus = orders
            .GroupBy(o => o.Status.ToString())
            .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() })
            .OrderByDescending(s => s.Count)
            .ToList();

        var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-6);
        var revenueByDay = orders
            .Where(o => o.CreatedAt.Date >= sevenDaysAgo)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyRevenueDto
            {
                Date       = g.Key.ToString("yyyy-MM-dd"),
                Revenue    = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new DashboardSummaryDto
        {
            TotalOrders      = orders.Count,
            TotalRevenue     = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
            PendingOrders    = orders.Count(o => PendingStatuses.Contains(o.Status)),
            FailedOrders     = orders.Count(o => o.Status == OrderStatus.Failed),
            CompletedOrders  = orders.Count(o => o.Status == OrderStatus.Completed),
            CancelledOrders  = orders.Count(o => o.Status == OrderStatus.Cancelled),
            ByStatus         = byStatus,
            RevenueLastSevenDays = revenueByDay
        };
    }
}
