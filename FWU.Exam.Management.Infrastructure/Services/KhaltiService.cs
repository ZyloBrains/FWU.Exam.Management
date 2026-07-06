using System.Text;
using System.Text.Json;
using FWU.Exam.Management.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class KhaltiService(HttpClient httpClient, IConfiguration configuration, ILogger<KhaltiService> logger) : IKhaltiService
{
    private string BaseUrl => configuration["Khalti:BaseUrl"] ?? "https://dev.khalti.com/api/v2";
    private string SecretKey => configuration["Khalti:SecretKey"] ?? "";

    public async Task<KhaltiInitiateResponse?> InitiatePaymentAsync(KhaltiInitiateRequest request)
    {
            var payload = new
            {
                return_url = request.ReturnUrl,
                website_url = string.IsNullOrEmpty(request.WebsiteUrl) ? (configuration["Khalti:WebsiteUrl"] ?? "") : request.WebsiteUrl,
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

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/epayment/initiate/")
        {
            Content = content
        };
        requestMessage.Headers.Add("Authorization", $"Key {SecretKey}");

        logger.LogInformation("Sending Khalti initiate request to {Url} with amount {Amount} paisa", $"{BaseUrl}/epayment/initiate/", request.Amount);

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
        var payload = new { pidx };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/epayment/lookup/")
        {
            Content = content
        };
        requestMessage.Headers.Add("Authorization", $"Key {SecretKey}");

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
