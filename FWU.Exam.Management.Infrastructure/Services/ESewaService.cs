using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ESewaService(AppDbContext context, HttpClient httpClient, IConfiguration configuration) : IESewaService
{
    public string GenerateTransactionUuid()
    {
        return $"{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}";
    }

    public async Task<ESewaPaymentFormData> GeneratePaymentFormDataAsync(decimal totalAmount, string transactionUuid, string successUrl, string failureUrl)
    {
        var config = await GetConfigAsync();
        var totalAmountStr = totalAmount.ToString("F0", CultureInfo.InvariantCulture);
        var signedFieldNames = "total_amount,transaction_uuid,product_code";
        var message = $"total_amount={totalAmountStr},transaction_uuid={transactionUuid},product_code={config.ProductCode}";
        var signature = GenerateSignature(message, config.SecretKey);

        return new ESewaPaymentFormData
        {
            PostUrl = config.PostUrl,
            Amount = totalAmountStr,
            TaxAmount = "0",
            TotalAmount = totalAmountStr,
            TransactionUuid = transactionUuid,
            ProductCode = config.ProductCode,
            ProductServiceCharge = config.ServiceChargeAmount.ToString("0", CultureInfo.InvariantCulture),
            ProductDeliveryCharge = "0",
            SuccessUrl = successUrl,
            FailureUrl = failureUrl,
            SignedFieldNames = signedFieldNames,
            Signature = signature
        };
    }

    public async Task<bool> VerifyResponseSignatureAsync(ESewaVerifyResponse response, string rawJson)
    {
        if (response.SignedFieldNames == null || response.Signature == null)
            return false;

        var config = await GetConfigAsync();
        var fieldNames = response.SignedFieldNames.Split(',', StringSplitOptions.TrimEntries);
        var messageParts = new List<string>();

        using var doc = JsonDocument.Parse(rawJson);
        var root = doc.RootElement;

        foreach (var field in fieldNames)
        {
            string value;
            if (root.TryGetProperty(field, out var prop))
            {
                value = prop.ValueKind switch
                {
                    JsonValueKind.String => prop.GetString() ?? "",
                    JsonValueKind.Number => prop.GetRawText(),
                    _ => prop.GetRawText()
                };
            }
            else
            {
                value = "";
            }
            messageParts.Add($"{field}={value}");
        }

        var message = string.Join(",", messageParts);
        var expectedSignature = GenerateSignature(message, config.SecretKey);
        return expectedSignature == response.Signature;
    }

    public async Task<ESewaVerifyResponse?> VerifyTransactionAsync(string transactionUuid, decimal totalAmount)
    {
        var config = await GetConfigAsync();

        var skipVerification = string.Equals(configuration["ESewaConfig:SkipVerification"], "true", StringComparison.OrdinalIgnoreCase);
        if (skipVerification)
        {
            return new ESewaVerifyResponse
            {
                TransactionUuid = transactionUuid,
                TotalAmount = totalAmount,
                Status = "COMPLETE",
                ProductCode = config.ProductCode
            };
        }

        const int maxAttempts = 2;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var url = $"{config.VerifyUrl}?product_code={config.ProductCode}&total_amount={totalAmount:F0}&transaction_uuid={transactionUuid}";
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ESewaVerifyResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString });
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                System.Console.WriteLine($"ESewa VerifyTransaction retry {attempt}: {ex.Message}");
            }
        }
        return null;
    }

    private static string GenerateSignature(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }

    private async Task<ESewaConfiguration> GetConfigAsync()
    {
        var currentTenantId = AppDbContext.GetCurrentTenantId();
        var config = await context.ESewaConfigurations
            .AsNoTracking()
            .OrderBy(c => c.TenantId != currentTenantId)
            .ThenBy(c => c.Id)
            .FirstOrDefaultAsync();

        if (config == null)
            throw new InvalidOperationException("eSewa configuration is not set up for this tenant.");

        return config;
    }
}
