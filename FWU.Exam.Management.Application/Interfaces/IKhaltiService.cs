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
    public string? Pidx { get; set; }
    public string? PaymentUrl { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? ExpiresIn { get; set; }
}

public class KhaltiLookupResponse
{
    public string? Pidx { get; set; }
    public long TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? TransactionId { get; set; }
    public long Fee { get; set; }
    public bool Refunded { get; set; }
}

public interface IKhaltiService
{
    Task<KhaltiInitiateResponse?> InitiatePaymentAsync(KhaltiInitiateRequest request);
    Task<KhaltiLookupResponse?> LookupPaymentAsync(string pidx);
}
