using System.Text.Json.Serialization;

namespace FWU.Exam.Management.Application.Interfaces;

public class KhaltiInitiateRequest
{
    public string ReturnUrl { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string PurchaseOrderId { get; set; } = string.Empty;
    public string PurchaseOrderName { get; set; } = string.Empty;
    public KhaltiCustomerInfo? CustomerInfo { get; set; }
}

public class KhaltiCustomerInfo
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class KhaltiInitiateResponse
{
    [JsonPropertyName("pidx")]
    public string? Pidx { get; set; }

    [JsonPropertyName("payment_url")]
    public string? PaymentUrl { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }
}

public class KhaltiLookupResponse
{
    [JsonPropertyName("pidx")]
    public string? Pidx { get; set; }

    [JsonPropertyName("total_amount")]
    public long TotalAmount { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("transaction_id")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("fee")]
    public long Fee { get; set; }

    [JsonPropertyName("refunded")]
    public bool Refunded { get; set; }
}

public interface IKhaltiService
{
    Task<KhaltiInitiateResponse?> InitiatePaymentAsync(KhaltiInitiateRequest request);
    Task<KhaltiLookupResponse?> LookupPaymentAsync(string pidx);
}
