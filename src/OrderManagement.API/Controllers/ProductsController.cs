using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.API.CQRS.Queries;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        var products = await mediator.Send(new GetProductsQuery(category));
        return Ok(products);
    }
}
