using System.Text;
using System.Text.Json;
using FWU.Exam.Management.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class KhaltiService(HttpClient httpClient, IKhaltiConfigurationService configService, ILogger<KhaltiService> logger) : IKhaltiService
{
    private Domain.Entities.Payments.KhaltiConfiguration? _config;
    private Domain.Entities.Payments.KhaltiConfiguration GetConfig()
    {
        if (_config != null) return _config;
        _config = configService.GetActiveAsync().GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("No active Khalti configuration found. Configure Khalti in the admin panel.");
        return _config;
    }

    public async Task<KhaltiInitiateResponse?> InitiatePaymentAsync(KhaltiInitiateRequest request)
    {
        var config = GetConfig();

        var payload = new
        {
            return_url = request.ReturnUrl,
            website_url = string.IsNullOrEmpty(request.WebsiteUrl) ? (config.WebsiteUrl ?? "") : request.WebsiteUrl,
            amount = request.Amount,
            purchase_order_id = request.PurchaseOrderId,
            purchase_order_name = request.PurchaseOrderName,
            customer_info = request.CustomerInfo == null ? null : new
            {
                name = request.CustomerInfo.Name,
                email = request.CustomerInfo.Email,
                phone = request.CustomerInfo.Phone
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{config.PostUrl}/epayment/initiate/")
        {
            Content = content
        };
        requestMessage.Headers.Add("Authorization", $"Key {config.AuthorizationKey}");

        logger.LogInformation("Sending Khalti initiate request to {Url} with amount {Amount} paisa", $"{config.PostUrl}/epayment/initiate/", request.Amount);

        var response = await httpClient.SendAsync(requestMessage);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = $"Khalti API returned {(int)response.StatusCode}: {responseJson}";
            logger.LogError("Khalti initiate failed: {Error}", errorDetail);
            throw new InvalidOperationException(errorDetail);
        }

        logger.LogInformation("Khalti initiate succeeded: {Response}", responseJson);
        return JsonSerializer.Deserialize<KhaltiInitiateResponse>(responseJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<KhaltiLookupResponse?> LookupPaymentAsync(string pidx)
    {
        var config = GetConfig();
        var payload = new { pidx };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{config.PostUrl}/epayment/lookup/")
        {
            Content = content
        };
        requestMessage.Headers.Add("Authorization", $"Key {config.AuthorizationKey}");

        logger.LogInformation("Sending Khalti lookup request for pidx={Pidx}", pidx);

        var response = await httpClient.SendAsync(requestMessage);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = $"Khalti lookup API returned {(int)response.StatusCode}: {responseJson}";
            logger.LogError("Khalti lookup failed: {Error}", errorDetail);
            throw new InvalidOperationException(errorDetail);
        }

        logger.LogInformation("Khalti lookup succeeded: {Response}", responseJson);
        return JsonSerializer.Deserialize<KhaltiLookupResponse>(responseJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
