using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.API.CQRS.Commands;
using OrderManagement.API.Services;
using Shared.Contracts.DTOs;

namespace OrderManagement.API.Controllers;

public record CreateStripeSessionDto(CheckoutRequestDto Request, string SuccessUrl, string CancelUrl);

[ApiController]
[Route("api/stripe")]
public class StripeController(
    IStripeService stripe,
    IMediator mediator,
    ILogger<StripeController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a Stripe Hosted Checkout session.
    /// Returns the Stripe URL the browser should redirect to.
    /// </summary>
    [HttpPost("create-session")]
    public async Task<IActionResult> CreateSession([FromBody] CreateStripeSessionDto dto)
    {
        try
        {
            var url = await stripe.CreateCheckoutSessionAsync(
                dto.Request, dto.SuccessUrl, dto.CancelUrl);

            return Ok(new { sessionUrl = url });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("SecretKey"))
        {
            logger.LogError("Stripe secret key is not configured.");
            return StatusCode(503, "Payment service is not configured. Please contact the administrator.");
        }
    }

    /// <summary>
    /// Called after Stripe redirects back with a session_id.
    /// Verifies payment and places the order into the existing pipeline.
    /// </summary>
    [HttpGet("complete")]
    public async Task<IActionResult> CompleteOrder([FromQuery] string session_id)
    {
        if (string.IsNullOrWhiteSpace(session_id))
            return BadRequest("session_id is required.");

        var (isPaid, paymentIntentId) = await stripe.VerifyPaymentAsync(session_id);

        if (!isPaid)
        {
            logger.LogWarning("Stripe session {SessionId} payment not confirmed.", session_id);
            return BadRequest(new { error = "Payment has not been completed." });
        }

        var request = stripe.GetPendingRequest(session_id);
        if (request is null)
        {
            logger.LogError("No pending request found for Stripe session {SessionId}.", session_id);
            return BadRequest(new { error = "Session data not found or has expired. Please try again." });
        }

        // Attach Stripe metadata so Payment.Service auto-approves
        request.StripeSessionId = session_id;
        request.PaymentIntentId = paymentIntentId;

        var order = await mediator.Send(new CheckoutOrderCommand(request));

        logger.LogInformation(
            "Order {OrderId} created after Stripe payment {SessionId}",
            order.OrderId, session_id);

        return Ok(order);
    }
}
