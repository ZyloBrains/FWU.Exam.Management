using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class KhaltiService(HttpClient httpClient, AppDbContext context, ILogger<KhaltiService> logger) : IKhaltiService
{
    public async Task<KhaltiInitiateResponse?> InitiatePaymentAsync(KhaltiInitiateRequest request)
    {
        var config = await GetConfigAsync();

        var payload = new
        {
            return_url = request.ReturnUrl,
            website_url = string.IsNullOrEmpty(request.WebsiteUrl) ? config.WebsiteUrl : request.WebsiteUrl,
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

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, config.PostUrl)
        {
            Content = content
        };
        requestMessage.Headers.Add("Authorization", $"Key {config.AuthorizationKey}");

        logger.LogInformation("Sending Khalti initiate request to {Url} with amount {Amount} paisa", config.PostUrl, request.Amount);

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
        var config = await GetConfigAsync();

        var payload = new { pidx };
        var json = JsonSerializer.Serialize(payload);

        logger.LogInformation("Sending Khalti lookup request for pidx={Pidx}", pidx);

        const int maxAttempts = 2;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            // A fresh request is required per attempt: HttpClient refuses to send
            // the same HttpRequestMessage twice ("The request message was already
            // sent"), which broke the retry path.
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, config.VerifyUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("Authorization", $"Key {config.AuthorizationKey}");

            try
            {
                var response = await httpClient.SendAsync(requestMessage);
                var responseJson = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (!response.IsSuccessStatusCode)
                {
                    // Khalti returns HTTP 400 with a valid status payload for some
                    // terminal states (e.g. Expired). Parse it so the caller can
                    // detect and close those instead of treating them as a hard
                    // failure that is retried forever.
                    try
                    {
                        var failed = JsonSerializer.Deserialize<KhaltiLookupResponse>(responseJson, options);
                        if (failed != null && !string.IsNullOrWhiteSpace(failed.Status))
                        {
                            logger.LogInformation("Khalti lookup returned status '{Status}' for pidx={Pidx} (HTTP {StatusCode})", failed.Status, pidx, (int)response.StatusCode);
                            return failed;
                        }
                    }
                    catch (Exception parseEx)
                    {
                        logger.LogWarning(parseEx, "Khalti lookup non-success response could not be interpreted: pidx={Pidx}", pidx);
                    }

                    var errorDetail = $"Khalti lookup API returned {(int)response.StatusCode}: {responseJson}";
                    logger.LogError("Khalti lookup failed: {Error}", errorDetail);
                    throw new InvalidOperationException(errorDetail);
                }

                logger.LogInformation("Khalti lookup succeeded: {Response}", responseJson);
                return JsonSerializer.Deserialize<KhaltiLookupResponse>(responseJson, options);
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(ex, "Khalti lookup retry {Attempt}: {Url}", attempt, config.VerifyUrl);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
        return null;
    }

    private async Task<KhaltiConfiguration> GetConfigAsync()
    {
        var config = await context.KhaltiConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (config == null)
            throw new InvalidOperationException("Khalti configuration is not set up for this tenant.");

        var problems = KhaltiConfigurationValidator.Validate(config);
        if (problems.Count > 0)
        {
            var detail = string.Join(" ", problems);
            logger.LogWarning("Khalti configuration is invalid: {Problems}", detail);
            throw new InvalidOperationException($"Khalti configuration is invalid. {detail}");
        }

        return config;
    }
}
