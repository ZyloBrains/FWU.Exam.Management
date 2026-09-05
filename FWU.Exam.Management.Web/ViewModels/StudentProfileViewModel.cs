namespace FWU.Exam.Management.Web.ViewModels;

public class StudentProfileViewModel
{
    public int RegistrationId { get; set; }
    public int? PermanentAddressId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? FullName { get; set; }
    public string? NepaliName { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirthBS { get; set; }
    public string? DateOfBirthAD { get; set; }
    public string? Ethnicity { get; set; }
    public string? Category { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public string? PhotoPath { get; set; }
    public string? SignaturePath { get; set; }
    public string? Address { get; set; }
    public string? BloodGroup { get; set; }
    public string? Nationality { get; set; }
    public string? Religion { get; set; }
    public string? AcademicYear { get; set; }
    public string? College { get; set; }
    public string? Level { get; set; }
}

public class ExamFormViewModel
{
    public int ExamScheduleId { get; set; }
    public string? Level { get; set; }
    public string? Program { get; set; }
    public string? Semester { get; set; }
    public string? ExamScheduleName { get; set; }
    public string? Status { get; set; }
    public decimal Amount { get; set; }
    public bool HasPaid { get; set; }
    public bool HasAdmitCard { get; set; }
    public int? AdmitCardId { get; set; }
    public bool IsRejected { get; set; }
    public bool IsPaymentUnderVerification { get; set; }
    public string? RejectionReason { get; set; }
    public string? EndDateBs { get; set; }
    public string? ExtendedDateBs { get; set; }
    public DateTime? AdmissionCardReleaseDate { get; set; }
}

public class ExamFormsListViewModel
{
    public List<ExamFormViewModel> ExamForms { get; set; } = new();
}

public class ReapplyExamViewModel
{
    public int ExamScheduleId { get; set; }
    public string? ExamScheduleName { get; set; }
    public string? SemesterName { get; set; }
    public string? ExamTypeName { get; set; }
    public string? EndDateBs { get; set; }
    public decimal PaidAmount { get; set; }
    public string? RejectionReason { get; set; }
    public List<SubjectFeeDetail> Subjects { get; set; } = new();
    public HashSet<int> PreSelectedSubjectIds { get; set; } = new();

    // Schedule rates for the client-side charge-delta preview. The server
    // recomputes everything authoritatively on submit.
    public decimal ExamFee { get; set; }
    public decimal PracticalFee { get; set; }

    // True when an earlier top-up attempt was left unpaid and will be
    // superseded by the next submission.
    public bool HasUnpaidTopUp { get; set; }

    // Partial (re-exam) schedules allow free per-leg choice; regular
    // schedules lock every tick to the previously paid selection.
    public bool IsPartialForm { get; set; }

    // Gateways the reapply top-up flow can settle (eSewa / Khalti).
    public List<PaymentTypeDetail> PaymentTypes { get; set; } = new();
}

public class SubjectFeeDetail
{
    public int SubjectOfferingId { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public bool HasTheory { get; set; }
    public bool HasPractical { get; set; }
    public decimal PracticalFee { get; set; }
    public bool IsSelected { get; set; }
    public bool IsFailed { get; set; }

    // Per-leg failure/selection state (re-exam forms). A subject whose
    // practical failed but theory passed shows FailedPractical=true and the
    // student may re-register just that leg.
    public bool FailedTheory { get; set; }
    public bool FailedPractical { get; set; }
    public bool SelectedTheory { get; set; }
    public bool SelectedPractical { get; set; }

    public bool IsCompulsory { get; set; }
    public int SubjectTypeId { get; set; }
    public string? SubjectTypeName { get; set; }
}

public class PaymentTypeDetail
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? LogoUrl { get; set; }
}

public class ExamPaymentViewModel
{
    public int ExamScheduleId { get; set; }
    public string? ExamScheduleName { get; set; }
    public string? ProgramName { get; set; }
    public string? SemesterName { get; set; }
    public string? StudentName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? EndDateBs { get; set; }
    public string? AcademicYearName { get; set; }
    public string? ExamTypeName { get; set; }
    public decimal TotalExamFee { get; set; }
    public decimal TotalPracticalFee { get; set; }
    public decimal ExtendedDateCharge { get; set; }
    public decimal GrandTotal { get; set; }
    public List<SubjectFeeDetail> Subjects { get; set; } = new();
    public bool HasESewa { get; set; }
    public bool HasKhalti { get; set; }
    public bool HasConnectIPS { get; set; }
    public bool IsRegular { get; set; }
    public List<int> SelectedSubjectIds { get; set; } = new();
    public List<PaymentTypeDetail> PaymentTypes { get; set; } = new();
}

public class MarksheetViewModel
{
    public string? RegistrationNumber { get; set; }
    public string? StudentName { get; set; }
    public string? Program { get; set; }
    public string? Faculty { get; set; }
    public string? ExamSchedule { get; set; }
    public string? Semester { get; set; }
    public int? SemesterId { get; set; }
    public int SemesterYear { get; set; }
    public int SemesterNumber { get; set; }
    public string? Level { get; set; }
    public string? ExamType { get; set; }
    public string? AcademicYear { get; set; }
    public string? College { get; set; }
    public List<MarksheetSubjectViewModel> Subjects { get; set; } = new();
    public string? TotalGpa { get; set; }
    public string? Result { get; set; }
    public string? TheoryGrade { get; set; }
    public string? PracticalGrade { get; set; }
    public string? SymbolNumber { get; set; }
    public int ExamScheduleId { get; set; }
    public int TotalPassed => Subjects.Count(s => s.IsPassed);
    public int TotalFailed => Subjects.Count(s => s.Status == "Fail");
    public int TotalPending => Subjects.Count(s => s.Status == "Pending");
}

public class MarksheetSubjectViewModel
{
    public int ExamSubjectResultId { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public float? CreditHours { get; set; }
    public float? TheoryMarks { get; set; }
    public float? PracticalMarks { get; set; }
    public float? InternalMarks { get; set; }
    public float? TotalMarks { get; set; }
    public string? Grade { get; set; }
    public decimal? GradeValue { get; set; }
    public decimal? GradePoint { get; set; }
    public string? TheoryGrade { get; set; }
    public decimal? TheoryGradePoint { get; set; }
    public string? PracticalGrade { get; set; }
    public decimal? PracticalGradePoint { get; set; }
    public bool IsPassed { get; set; }
    public string? Status { get; set; }
}

public class ExamFormAdminViewModel
{
    public int ExamRegistrationId { get; set; }
    public string? StudentName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? CollegeName { get; set; }
    public int? ExamScheduleId { get; set; }
    public string? ExamScheduleName { get; set; }
    public string? ProgramName { get; set; }
    public decimal? FeeEnclosed { get; set; }
    public Domain.Enums.RegistrationStatus Status { get; set; }
    public bool PaymentConfirmed { get; set; }
    public string? InvoiceNumber { get; set; }
    public bool HasAdmitCard { get; set; }
    public DateTime? RegistrationDate { get; set; }
}

public class ExamFormsAdminListViewModel
{
    public List<ExamFormAdminViewModel> Forms { get; set; } = new();
    public int TotalCount { get; set; }
    public int PaymentConfirmedCount { get; set; }
    public int AdmitCardGeneratedCount { get; set; }
    public int PendingAdmitCardCount { get; set; }
}
