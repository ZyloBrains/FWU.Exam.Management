namespace FWU.Exam.Management.Application.DTOs;

public class TheoryMarksPageViewModel
{
    public bool IsSuperAdmin { get; set; }
    public bool IsFacultyAdmin { get; set; }
    public List<SelectOption> Faculties { get; set; } = [];
    public List<SelectOption> Colleges { get; set; } = [];
}

public class StudentTheoryMarksRowDto
{
    public int ExamRegistrationId { get; set; }
    public int? ExamSubjectResultId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string SymbolNumber { get; set; } = string.Empty;
    public float? Theory { get; set; }
    public bool IsSubmitted { get; set; }
}

public class StudentTheoryMarksViewModel
{
    public int ExamScheduleId { get; set; }
    public int SubjectOfferingId { get; set; }
    public float TheoryFullMarks { get; set; }
    public float TheoryPassMarks { get; set; }
    public List<StudentTheoryMarksRowDto> Students { get; set; } = [];
}

public class TheoryMarksSaveDto
{
    public int CollegeId { get; set; }
    public int ExamScheduleId { get; set; }
    public int SubjectOfferingId { get; set; }
    public bool SubmitAll { get; set; }
    public List<StudentTheoryMarksRowDto> Students { get; set; } = [];
}
