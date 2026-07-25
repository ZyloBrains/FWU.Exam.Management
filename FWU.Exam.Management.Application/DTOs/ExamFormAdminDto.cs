using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Application.DTOs;

public class ExamFormAdminDto
{
    public int ExamRegistrationId { get; set; }
    public string? StudentName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? CollegeName { get; set; }
    public int? ExamScheduleId { get; set; }
    public string? ExamScheduleName { get; set; }
    public string? ProgramName { get; set; }
    public decimal? FeeEnclosed { get; set; }
    public RegistrationStatus Status { get; set; }
    public bool PaymentConfirmed { get; set; }
    public string? InvoiceNumber { get; set; }
    public bool HasAdmitCard { get; set; }
    public DateTime? RegistrationDate { get; set; }
}

public class ExamFormsAdminResult
{
    public List<ExamFormAdminDto> Forms { get; set; } = new();
    public int TotalCount { get; set; }
    public int PaymentConfirmedCount { get; set; }
    public int AdmitCardGeneratedCount { get; set; }
    public int PendingAdmitCardCount { get; set; }
}
