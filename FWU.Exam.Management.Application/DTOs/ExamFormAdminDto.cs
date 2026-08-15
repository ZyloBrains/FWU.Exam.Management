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
    public string? AcademicYearName { get; set; }
    public string? LevelName { get; set; }
    public string? SemesterName { get; set; }
    public string? ExamTypeName { get; set; }
    public string? DateOfBirthAD { get; set; }
    public string? ContactNumber { get; set; }
    public decimal? FeeEnclosed { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? PhotoPath { get; set; }
    public string? SignaturePath { get; set; }
    public List<ExamFormSubjectDto> Subjects { get; set; } = [];
    public RegistrationStatus Status { get; set; }
    public bool PaymentConfirmed { get; set; }
    public string? InvoiceNumber { get; set; }
    public bool HasAdmitCard { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public string? VerifiedByUsername { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public bool CanApprove { get; set; }
    public bool CanAdminApprove { get; set; }
}

public class ExamFormSubjectDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool Theory { get; set; }
    public bool Practical { get; set; }
}

public class ExamFormsAdminResult
{
    public List<ExamFormAdminDto> Forms { get; set; } = new();
    public int TotalCount { get; set; }
    public int PaymentConfirmedCount { get; set; }
    public int AdmitCardGeneratedCount { get; set; }
    public int PendingAdmitCardCount { get; set; }
    public int PendingApprovalCount { get; set; }
    public List<SchedulePendingCountDto> PendingBySchedule { get; set; } = new();
}

public class SchedulePendingCountDto
{
    public string ScheduleName { get; set; } = string.Empty;
    public int PendingCount { get; set; }
}
