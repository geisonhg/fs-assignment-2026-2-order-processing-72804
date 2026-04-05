using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.API.CQRS.Queries;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet("{customerId:int}/orders")]
    public async Task<IActionResult> GetOrders(int customerId)
    {
        var orders = await mediator.Send(new GetCustomerOrdersQuery(customerId));
        return Ok(orders);
    }
}
