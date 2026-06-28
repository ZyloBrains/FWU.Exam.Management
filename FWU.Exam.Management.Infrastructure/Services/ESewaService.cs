using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FWU.Exam.Management.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ESewaService(IConfiguration configuration, HttpClient httpClient) : IESewaService
{
    private string PostUrl => configuration["ESewa:PostUrl"] ?? "https://rc-epay.esewa.com.np/api/epay/main/v2/form";
    private string ProductCode => configuration["ESewa:ProductCode"] ?? "EPAYTEST";
    private string SecretKey => configuration["ESewa:SecretKey"] ?? throw new InvalidOperationException("ESewa:SecretKey is not configured");
    private string ServiceChargeAmount => configuration["ESewa:ServiceChargeAmount"] ?? "0";
    private string VerifyUrl => configuration["ESewa:VerifyUrl"] ?? "https://rc-epay.esewa.com.np/api/epay/transaction/status/";

    public string GenerateTransactionUuid()
    {
        return $"{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}";
    }

    public string GenerateSignature(string message)
    {
        var keyBytes = Encoding.UTF8.GetBytes(SecretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public ESewaPaymentFormData GeneratePaymentFormData(decimal totalAmount, string transactionUuid, string successUrl, string failureUrl)
    {
        var totalAmountStr = totalAmount.ToString("F0");
        var signedFieldNames = "total_amount,transaction_uuid,product_code";
        var message = $"total_amount={totalAmountStr},transaction_uuid={transactionUuid},product_code={ProductCode}";
        var signature = GenerateSignature(message);

        return new ESewaPaymentFormData
        {
            PostUrl = PostUrl,
            Amount = totalAmountStr,
            TaxAmount = "0",
            TotalAmount = totalAmountStr,
            TransactionUuid = transactionUuid,
            ProductCode = ProductCode,
            ProductServiceCharge = ServiceChargeAmount,
            ProductDeliveryCharge = "0",
            SuccessUrl = successUrl,
            FailureUrl = failureUrl,
            SignedFieldNames = signedFieldNames,
            Signature = signature
        };
    }

    public bool VerifyResponseSignature(ESewaVerifyResponse response, string rawJson)
    {
        if (response.SignedFieldNames == null || response.Signature == null)
            return false;

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
        var expectedSignature = GenerateSignature(message);
        return expectedSignature == response.Signature;
    }

    public async Task<ESewaVerifyResponse?> VerifyTransactionAsync(string transactionUuid, decimal totalAmount)
    {
        try
        {
            var url = $"{VerifyUrl}?product_code={ProductCode}&total_amount={totalAmount:F0}&transaction_uuid={transactionUuid}";
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
}
