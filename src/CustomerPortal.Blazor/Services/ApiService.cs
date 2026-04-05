using System.Net.Http.Json;
using System.Text.Json;
using Shared.Contracts.DTOs;

namespace CustomerPortal.Blazor.Services;

public class ApiService(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        var result = await http.GetFromJsonAsync<List<ProductDto>>("api/products", JsonOptions);
        return result ?? [];
    }

    public async Task<OrderDto> CheckoutAsync(CheckoutRequestDto request)
    {
        var response = await http.PostAsJsonAsync("api/orders/checkout", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions))!;
    }

    public async Task<OrderDto?> GetOrderAsync(int orderId)
    {
        return await http.GetFromJsonAsync<OrderDto>($"api/orders/{orderId}", JsonOptions);
    }

    public async Task<List<OrderSummaryDto>> GetCustomerOrdersAsync(int customerId)
    {
        var result = await http.GetFromJsonAsync<List<OrderSummaryDto>>($"api/customers/{customerId}/orders", JsonOptions);
        return result ?? [];
    }
}
