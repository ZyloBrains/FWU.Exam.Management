using System.Text.Json.Serialization;
using FWU.Exam.Management.Domain.Entities.Payments;

namespace FWU.Exam.Management.Application.Interfaces;

public class ESewaPaymentFormData
{
    public string PostUrl { get; set; } = string.Empty;
    public string Amount { get; set; } = "0";
    public string TaxAmount { get; set; } = "0";
    public string TotalAmount { get; set; } = "0";
    public string TransactionUuid { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductServiceCharge { get; set; } = "0";
    public string ProductDeliveryCharge { get; set; } = "0";
    public string SuccessUrl { get; set; } = string.Empty;
    public string FailureUrl { get; set; } = string.Empty;
    public string SignedFieldNames { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

public class ESewaVerifyResponse
{
    [JsonPropertyName("transaction_code")]
    public string? TransactionCode { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }
    [JsonPropertyName("transaction_uuid")]
    public string? TransactionUuid { get; set; }
    [JsonPropertyName("product_code")]
    public string? ProductCode { get; set; }
    [JsonPropertyName("signed_field_names")]
    public string? SignedFieldNames { get; set; }
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

public interface IESewaService
{
    ESewaPaymentFormData GeneratePaymentFormData(decimal totalAmount, string transactionUuid, string successUrl, string failureUrl);
    string GenerateSignature(string message);
    bool VerifyResponseSignature(ESewaVerifyResponse response, string rawJson);
    string GenerateTransactionUuid();
    Task<ESewaVerifyResponse?> VerifyTransactionAsync(string transactionUuid, decimal totalAmount);
}
