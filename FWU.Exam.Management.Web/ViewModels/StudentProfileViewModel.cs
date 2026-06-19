namespace FWU.Exam.Management.Web.ViewModels;

public class StudentProfileViewModel
{
    public int RegistrationId { get; set; }
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
    public string? PhotoPath { get; set; }
    public string? SignaturePath { get; set; }
    public string? Address { get; set; }
    public string? BloodGroup { get; set; }
    public string? Nationality { get; set; }
    public string? Religion { get; set; }
    public string? AcademicYear { get; set; }
    public string? Department { get; set; }
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
}

public class ExamFormsListViewModel
{
    public List<ExamFormViewModel> ExamForms { get; set; } = new();
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
    public bool IsCompulsory { get; set; }
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
    public decimal TotalExamFee { get; set; }
    public decimal TotalPracticalFee { get; set; }
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
    public string? ExamSchedule { get; set; }
    public string? AcademicYear { get; set; }
    public string? College { get; set; }
    public List<MarksheetSubjectViewModel> Subjects { get; set; } = new();
    public string? TotalGpa { get; set; }
    public string? Result { get; set; }
}

public class MarksheetSubjectViewModel
{
    public string? SubjectName { get; set; }
    public string? TheoryMarks { get; set; }
    public string? PracticalMarks { get; set; }
    public string? InternalMarks { get; set; }
    public string? TotalMarks { get; set; }
    public string? Grade { get; set; }
    public string? GradePoint { get; set; }
}
