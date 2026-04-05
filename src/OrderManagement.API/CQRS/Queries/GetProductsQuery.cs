using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Data;
using Shared.Contracts.DTOs;

namespace OrderManagement.API.CQRS.Queries;

public record GetProductsQuery(string? Category = null) : IRequest<List<ProductDto>>;

public class GetProductsHandler(OrderDbContext db, IMapper mapper)
    : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(GetProductsQuery query, CancellationToken ct)
    {
        var products = db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Category))
            products = products.Where(p => p.Category == query.Category);

        var result = await products.OrderBy(p => p.Name).ToListAsync(ct);
        return mapper.Map<List<ProductDto>>(result);
    }
}
