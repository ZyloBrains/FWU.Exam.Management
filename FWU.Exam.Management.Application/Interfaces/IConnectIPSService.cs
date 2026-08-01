using System.Text.Json.Serialization;

namespace FWU.Exam.Management.Application.Interfaces;

public class ConnectIpsFormData
{
    public string FormActionUrl { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string TxnId { get; set; } = string.Empty;
    public string TxnDate { get; set; } = string.Empty;
    public string TxnCurrency { get; set; } = "NPR";
    public string TxnAmt { get; set; } = "0";
    public string ReferenceId { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public string Particulars { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class ConnectIpsValidateResponse
{
    [JsonPropertyName("merchantId")]
    public string? MerchantId { get; set; }

    [JsonPropertyName("appId")]
    public string? AppId { get; set; }

    [JsonPropertyName("referenceId")]
    public string? ReferenceId { get; set; }

    [JsonPropertyName("txnAmt")]
    public string? TxnAmt { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("statusDesc")]
    public string? StatusDesc { get; set; }
}

public interface IConnectIPSService
{
    Task<ConnectIpsFormData?> GeneratePaymentFormDataAsync(decimal amountNpr, string txnId, string referenceId, string remarks, string particulars);
    Task<ConnectIpsValidateResponse?> ValidateTransactionAsync(string txnId, decimal amountNpr);
}
