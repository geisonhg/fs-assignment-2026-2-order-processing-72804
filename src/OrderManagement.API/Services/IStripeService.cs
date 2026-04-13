using Shared.Contracts.DTOs;

namespace OrderManagement.API.Services;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(CheckoutRequestDto request, string successUrl, string cancelUrl);
    Task<(bool IsPaid, string? PaymentIntentId)> VerifyPaymentAsync(string sessionId);
    CheckoutRequestDto? GetPendingRequest(string sessionId);
}
