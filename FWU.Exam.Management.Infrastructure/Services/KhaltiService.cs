using System.Text;
using System.Text.Json;
using FWU.Exam.Management.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FWU.Exam.Management.Infrastructure.Services;

public class KhaltiService(HttpClient httpClient, IConfiguration configuration) : IKhaltiService
{
    private string BaseUrl => configuration["Khalti:BaseUrl"] ?? "https://dev.khalti.com/api/v2";
    private string SecretKey => configuration["Khalti:SecretKey"] ?? "";
    private string WebsiteUrl => configuration["Khalti:WebsiteUrl"] ?? "";

    public async Task<KhaltiInitiateResponse?> InitiatePaymentAsync(KhaltiInitiateRequest request)
    {
        try
        {
            var payload = new
            {
                return_url = request.ReturnUrl,
                website_url = WebsiteUrl,
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

            var response = await httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<KhaltiInitiateResponse>(responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }

    public async Task<KhaltiLookupResponse?> LookupPaymentAsync(string pidx)
    {
        try
        {
            var payload = new { pidx };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/epayment/lookup/")
            {
                Content = content
            };
            requestMessage.Headers.Add("Authorization", $"Key {SecretKey}");

            var response = await httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<KhaltiLookupResponse>(responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }
}
