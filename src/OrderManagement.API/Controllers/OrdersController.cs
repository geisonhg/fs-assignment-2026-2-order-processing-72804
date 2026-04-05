using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.API.CQRS.Commands;
using OrderManagement.API.CQRS.Queries;
using Shared.Contracts.DTOs;
using Shared.Contracts.Enums;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto request)
    {
        var order = await mediator.Send(new CheckoutOrderCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] OrderStatus? status = null)
    {
        var orders = await mediator.Send(new GetOrdersQuery(status));
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await mediator.Send(new GetOrderByIdQuery(id));
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("{id:int}/status")]
    public async Task<IActionResult> GetStatus(int id)
    {
        var order = await mediator.Send(new GetOrderByIdQuery(id));
        if (order is null) return NotFound();
        return Ok(new { order.OrderId, order.Status, order.StatusDisplay, order.UpdatedAt });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var success = await mediator.Send(new CancelOrderCommand(id));
        return success ? NoContent() : BadRequest("Order cannot be cancelled at this stage.");
    }
}
