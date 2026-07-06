namespace FWU.Exam.Management.Application.DTOs;

public class TeacherDashboardDto
{
    public List<TeacherSubjectInfo> AssignedSubjects { get; set; } = [];
}

public class TeacherSubjectInfo
{
    public int SubjectOfferingId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public int RegisteredStudentCount { get; set; }
    public int MarksEnteredCount { get; set; }
}

public class MarksEntryViewModel
{
    public int SubjectOfferingId { get; set; }
    public int ExamScheduleId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public float TheoryFullMarks { get; set; }
    public float TheoryPassMarks { get; set; }
    public float? PracticalFullMarks { get; set; }
    public float? PracticalPassMarks { get; set; }
    public bool HasTheory { get; set; }
    public bool HasPractical { get; set; }
    public bool HasInternal { get; set; }
    public List<StudentMarksRowDto> Students { get; set; } = [];
}

public class StudentMarksRowDto
{
    public int ExamRegistrationId { get; set; }
    public int? ExamSubjectResultId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string SymbolNumber { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? TheoryMarks { get; set; }
    public string? TheoryConfirm { get; set; }
    public string? PracticalMarks { get; set; }
    public string? PracticalConfirm { get; set; }
    public decimal? TheoryInternal { get; set; }
    public decimal? PracticalInternal { get; set; }
    public decimal? TotalMarks { get; set; }
    public string? GradeLetter { get; set; }
    public bool IsPass { get; set; }
    public bool IsSubmitted { get; set; }
}

public class BulkMarksSaveDto
{
    public int SubjectOfferingId { get; set; }
    public int ExamScheduleId { get; set; }
    public bool SubmitAll { get; set; }
    public List<StudentMarksRowDto> Students { get; set; } = [];
}

public class ExcelImportResultDto
{
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = [];
}

public class SelectListOption
{
    public int Value { get; set; }
    public string Text { get; set; } = string.Empty;
}
