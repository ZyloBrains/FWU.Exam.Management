namespace FWU.Exam.Management.Application.DTOs;

public class PublishResultsPreviewDto
{
    public int ExamScheduleId { get; set; }
    public string? ExamScheduleName { get; set; }
    public int CollegeId { get; set; }
    public string? CollegeName { get; set; }
    public string? ProgramName { get; set; }
    public string? SemesterName { get; set; }
    public string? AcademicYearName { get; set; }
    public List<PublishResultsStudentDto> Students { get; set; } = [];
    public int TotalStudents { get; set; }
    public int SubjectsCount { get; set; }
}

public class PublishResultsStudentDto
{
    public int ExamRegistrationId { get; set; }
    public string? StudentName { get; set; }
    public string? SymbolNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? DateOfBirthBs { get; set; }
    public string? Sex { get; set; }
    public List<PublishResultsSubjectDto> Subjects { get; set; } = [];
    public decimal? GPA { get; set; }
    public string? Result { get; set; }
}

public class PublishResultsSubjectDto
{
    public int SubjectOfferingId { get; set; }
    public string? SubjectCode { get; set; }
    public string? SubjectName { get; set; }
    public int? CreditHours { get; set; }
    public float? TheoryMarks { get; set; }
    public float? InternalMarks { get; set; }
    public float? PracticalMarks { get; set; }
    public float? TotalMarks { get; set; }
    public string? GradeLetter { get; set; }
    public decimal? GradePoint { get; set; }
}

public class PublishResultsConfirmDto
{
    public int ExamScheduleId { get; set; }
    public int CollegeId { get; set; }
    public string PublishedBy { get; set; } = string.Empty;
}

public class PublishResultsResultDto
{
    public bool Success { get; set; }
    public int RecordsCreated { get; set; }
    public string? Message { get; set; }
}
