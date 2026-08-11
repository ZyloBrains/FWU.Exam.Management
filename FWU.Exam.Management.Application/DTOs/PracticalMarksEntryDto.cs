namespace FWU.Exam.Management.Application.DTOs;

public class PracticalMarksPageViewModel
{
    public bool IsSuperAdmin { get; set; }
    public bool IsFacultyAdmin { get; set; }
    public List<SelectOption> Faculties { get; set; } = [];
    public List<SelectOption> Colleges { get; set; } = [];
}

public class StudentPracticalMarksRowDto
{
    public int ExamRegistrationId { get; set; }
    public int? ExamSubjectResultId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string SymbolNumber { get; set; } = string.Empty;
    public float? Practical { get; set; }
    public bool IsSubmitted { get; set; }
}

public class StudentPracticalMarksViewModel
{
    public int ExamScheduleId { get; set; }
    public int SubjectOfferingId { get; set; }
    public float? PracticalFullMarks { get; set; }
    public float? PracticalPassMarks { get; set; }
    public List<StudentPracticalMarksRowDto> Students { get; set; } = [];
}

public class PracticalMarksSaveDto
{
    public int CollegeId { get; set; }
    public int ExamScheduleId { get; set; }
    public int SubjectOfferingId { get; set; }
    public bool SubmitAll { get; set; }
    public List<StudentPracticalMarksRowDto> Students { get; set; } = [];
}
