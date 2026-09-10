namespace FWU.Exam.Management.Application.DTOs;

public class PaymentHistoryListDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Gateway { get; set; }
    public string? TransactionId { get; set; }
    public string? ProviderReferenceId { get; set; }
    public DateTime ForwardedTime { get; set; }
    public string? ExamName { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public int? PaymentRequestLogStatus { get; set; }
}

public class PaymentHistoryResponseDto
{
    public int Id { get; set; }
    public DateTime ResponseTimestamp { get; set; }
    public bool IsSuccess { get; set; }
    public string ResponseMessage { get; set; } = string.Empty;
    public string FullResponse { get; set; } = string.Empty;
}

public class PaymentHistoryDetailDto : PaymentHistoryListDto
{
    public string? PaymentStatus { get; set; }
    public string? PaymentProvider { get; set; }
    public DateTime? InitiatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? SelectedSubjectIds { get; set; }
    public string? FullRequestContent { get; set; }
    public List<PaymentHistoryResponseDto> Responses { get; set; } = [];
}