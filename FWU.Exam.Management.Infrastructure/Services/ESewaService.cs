using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ESewaService(AppDbContext context, IConfiguration configuration, HttpClient httpClient) : IESewaService
{
    public string GenerateTransactionUuid()
    {
        return $"{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}";
    }

    public async Task<ESewaPaymentFormData> GeneratePaymentFormDataAsync(decimal totalAmount, string transactionUuid, string successUrl, string failureUrl)
    {
        var config = await GetConfigAsync();

        var postUrl = config?.PostUrl ?? configuration["ESewa:PostUrl"] ?? "https://rc-epay.esewa.com.np/api/epay/main/v2/form";
        var productCode = config?.ProductCode ?? configuration["ESewa:ProductCode"] ?? "EPAYTEST";
        var secretKey = config?.SecretKey ?? configuration["ESewa:SecretKey"] ?? throw new InvalidOperationException("eSewa secret key is not configured");
        var serviceChargeAmount = config?.ServiceChargeAmount.ToString("F0") ?? configuration["ESewa:ServiceChargeAmount"] ?? "0";

        var totalAmountStr = totalAmount.ToString("F0");
        var signedFieldNames = "total_amount,transaction_uuid,product_code";
        var message = $"total_amount={totalAmountStr},transaction_uuid={transactionUuid},product_code={productCode}";
        var signature = GenerateSignature(message, secretKey);

        return new ESewaPaymentFormData
        {
            PostUrl = postUrl,
            Amount = totalAmountStr,
            TaxAmount = "0",
            TotalAmount = totalAmountStr,
            TransactionUuid = transactionUuid,
            ProductCode = productCode,
            ProductServiceCharge = serviceChargeAmount,
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
        var secretKey = config?.SecretKey ?? configuration["ESewa:SecretKey"] ?? throw new InvalidOperationException("eSewa secret key is not configured");

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
        var expectedSignature = GenerateSignature(message, secretKey);
        return expectedSignature == response.Signature;
    }

    public async Task<ESewaVerifyResponse?> VerifyTransactionAsync(string transactionUuid, decimal totalAmount)
    {
        if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase))
        {
            var devConfig = await GetConfigAsync();
            return new ESewaVerifyResponse
            {
                TransactionUuid = transactionUuid,
                TotalAmount = totalAmount,
                Status = "COMPLETE",
                ProductCode = devConfig?.ProductCode ?? configuration["ESewa:ProductCode"] ?? "EPAYTEST"
            };
        }

        try
        {
            var config = await GetConfigAsync();
            var productCode = config?.ProductCode ?? configuration["ESewa:ProductCode"] ?? "EPAYTEST";
            var verifyUrl = config?.VerifyUrl ?? configuration["ESewa:VerifyUrl"] ?? "https://rc-epay.esewa.com.np/api/epay/transaction/status/";

            var url = $"{verifyUrl}?product_code={productCode}&total_amount={totalAmount:F0}&transaction_uuid={transactionUuid}";
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ESewaVerifyResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString });
        }
        catch
        {
            return null;
        }
    }

    private async Task<ESewaConfiguration?> GetConfigAsync()
    {
        return await context.ESewaConfigurations!.AsNoTracking().OrderBy(c => c.Id).FirstOrDefaultAsync();
    }

    private static string GenerateSignature(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
