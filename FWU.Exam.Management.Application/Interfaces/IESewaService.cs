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
    public string? TransactionCode { get; set; }
    public string? Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? TransactionUuid { get; set; }
    public string? ProductCode { get; set; }
    public string? SignedFieldNames { get; set; }
    public string? Signature { get; set; }
}

public interface IESewaService
{
    ESewaPaymentFormData GeneratePaymentFormData(decimal totalAmount, string transactionUuid, string successUrl, string failureUrl);
    string GenerateSignature(string message);
    bool VerifyResponseSignature(ESewaVerifyResponse response);
    string GenerateTransactionUuid();
    Task<ESewaVerifyResponse?> VerifyTransactionAsync(string transactionUuid, decimal totalAmount);
}
