namespace FWU.Exam.Management.Application.DTOs;

public class PaymentReconciliationListDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Gateway { get; set; }
    public string? TransactionId { get; set; }
    public DateTime ForwardedTime { get; set; }
    public string? ExamName { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
}

public class PaymentReconciliationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string GatewayStatus { get; set; } = string.Empty;
}

public class PaymentReconciliationOutcome
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Gateway { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PaymentReconciliationBatchResult
{
    public bool AlreadyRunning { get; set; }
    public int TotalPending { get; set; }
    public int Confirmed { get; set; }
    public int Expired { get; set; }
    public int Failed { get; set; }
    public int StillPending { get; set; }
    public List<PaymentReconciliationOutcome> Items { get; set; } = [];
}
