using Microsoft.Extensions.Caching.Memory;
using Shared.Contracts.DTOs;
using Stripe;
using Stripe.Checkout;

namespace OrderManagement.API.Services;

public class StripeService(
    IConfiguration config,
    IMemoryCache cache,
    ILogger<StripeService> logger) : IStripeService
{
    public async Task<string> CreateCheckoutSessionAsync(
        CheckoutRequestDto request, string successUrl, string cancelUrl)
    {
        var secretKey = config["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");

        StripeConfiguration.ApiKey = secretKey;

        var lineItems = request.Items.Select(i => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency   = "eur",
                UnitAmount = (long)(i.UnitPrice * 100),
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = i.ProductName
                }
            },
            Quantity = i.Quantity
        }).ToList();

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems          = lineItems,
            Mode               = "payment",
            SuccessUrl         = successUrl,
            CancelUrl          = cancelUrl
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        // Store the checkout request keyed by Stripe session ID (2 hours TTL)
        cache.Set(session.Id, request, TimeSpan.FromHours(2));

        logger.LogInformation(
            "Stripe session {SessionId} created — {Count} item(s), total €{Total:N2}",
            session.Id, request.Items.Count, request.Items.Sum(i => i.UnitPrice * i.Quantity));

        return session.Url;
    }

    public async Task<(bool IsPaid, string? PaymentIntentId)> VerifyPaymentAsync(string sessionId)
    {
        var secretKey = config["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured.");

        StripeConfiguration.ApiKey = secretKey;

        var service = new SessionService();
        var session = await service.GetAsync(sessionId);

        logger.LogInformation(
            "Stripe session {SessionId} status: {PaymentStatus}", sessionId, session.PaymentStatus);

        return (session.PaymentStatus == "paid", session.PaymentIntentId);
    }

    public CheckoutRequestDto? GetPendingRequest(string sessionId)
        => cache.TryGetValue(sessionId, out CheckoutRequestDto? req) ? req : null;
}
