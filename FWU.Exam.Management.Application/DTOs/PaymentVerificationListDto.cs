using System;
using System.Collections.Generic;

namespace FWU.Exam.Management.Application.DTOs;

public class PaymentVerificationListDto
{
    public int Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? VoucherDate { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public string? Branch { get; set; }
    public string? TransactionCode { get; set; }
    public string? PaymentGateway { get; set; }
    public DateTime? RequestedTime { get; set; }
    public string? RollNumber { get; set; }
    public string? ExamName { get; set; }
    public string? AcademicYear { get; set; }
    public string? Program { get; set; }
    public string? College { get; set; }
}
