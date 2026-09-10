namespace FWU.Exam.Management.Application.DTOs;

public class BulkMarksSaveDto
{
    public int SubjectOfferingId { get; set; }
    public int ExamScheduleId { get; set; }
    public bool SubmitAll { get; set; }
    public List<StudentMarksRowDto> Students { get; set; } = [];
}

public class StudentMarksRowDto
{
    public int ExamRegistrationId { get; set; }
    public int? ExamSubjectResultId { get; set; }
    public string StudentName { get; set; } = "";
    public string SymbolNumber { get; set; } = "";
    public string RegistrationNumber { get; set; } = "";
    public float? TheoryMarks { get; set; }
    public float? TheoryConfirm { get; set; }
    public float? PracticalMarks { get; set; }
    public float? PracticalConfirm { get; set; }
    public float? TheoryInternal { get; set; }
    public float? PracticalInternal { get; set; }
    public float? TotalMarks { get; set; }
    public string? GradeLetter { get; set; }
    public bool IsSubmitted { get; set; }
}

public class CollegeAdminDashboardDto
{
    public string? CollegeAdminUserId { get; set; }
    public int TotalAssignedSubjects { get; set; }
    public int PendingMarksSubmissions { get; set; }
    public int CompletedMarksSubmissions { get; set; }
    public int TotalStudentsWithMarks { get; set; }
    public List<CollegeAdminSubjectInfo> AssignedSubjects { get; set; } = [];
}

public class CollegeAdminSubjectInfo
{
    public int SubjectOfferingId { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public string? ProgramName { get; set; }
    public string? SemesterName { get; set; }
    public int RegisteredStudentCount { get; set; }
    public int MarksEnteredCount { get; set; }
}

public class MarksEntryViewModel
{
    public int SubjectOfferingId { get; set; }
    public int ExamScheduleId { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public bool HasTheory { get; set; }
    public bool HasPractical { get; set; }
    public bool HasInternal { get; set; }
    public float? TheoryFullMarks { get; set; }
    public float? TheoryPassMarks { get; set; }
    public float? PracticalFullMarks { get; set; }
    public float? PracticalPassMarks { get; set; }
    public float? InternalTheoryFullMarks { get; set; }
    public float? InternalPracticalFullMarks { get; set; }
    public List<StudentMarksRowDto> Students { get; set; } = [];
}

public class ExcelImportResultDto
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}


