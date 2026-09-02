namespace FWU.Exam.Management.Application.DTOs;

public class InternalMarksPageViewModel
{
    public bool IsSuperAdmin { get; set; }
    public bool IsFacultyAdmin { get; set; }
    public bool IsCollegeAdmin { get; set; }
    public int? CollegeId { get; set; }
    public List<SelectOption> Faculties { get; set; } = [];
    public List<SelectOption> Colleges { get; set; } = [];
    public List<SelectOption> AcademicYears { get; set; } = [];
}

public class ScheduleDetailDto
{
    public int ExamScheduleId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public string LevelName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public string ExamTypeName { get; set; } = string.Empty;
}

public class SubjectOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool HasTheory { get; set; }
    public bool HasPractical { get; set; }
    public float TheoryFullMarks { get; set; }
    public float? InternalTheoryFullMarks { get; set; }
    public float? PracticalFullMarks { get; set; }
    public float? PracticalPassMarks { get; set; }
}

public class SubjectDetailDto
{
    public int SubjectOfferingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool HasTheory { get; set; }
    public bool HasPractical { get; set; }
    public bool HasInternal { get; set; }
    public float TheoryFullMarks { get; set; }
    public float TheoryPassMarks { get; set; }
    public float? InternalTheoryFullMarks { get; set; }
    public float? InternalTheoryPassMarks { get; set; }
    public float? PracticalFullMarks { get; set; }
    public float? PracticalPassMarks { get; set; }
}

public class StudentInternalMarksRowDto
{
    public int ExamRegistrationId { get; set; }
    public int? ExamSubjectResultId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string SymbolNumber { get; set; } = string.Empty;
    public float? TheoryInternal { get; set; }
    public float? PracticalInternal { get; set; }
    public bool IsSubmitted { get; set; }
}

public class StudentInternalMarksViewModel
{
    public int ExamScheduleId { get; set; }
    public int SubjectOfferingId { get; set; }
    public bool HasPractical { get; set; }
    public float? InternalTheoryFullMarks { get; set; }
    public List<StudentInternalMarksRowDto> Students { get; set; } = [];
}

public class InternalMarksSaveDto
{
    public int CollegeId { get; set; }
    public int ExamScheduleId { get; set; }
    public int SubjectOfferingId { get; set; }
    public bool SubmitAll { get; set; }
    public List<StudentInternalMarksRowDto> Students { get; set; } = [];
}

public class SelectListOption
{
    public int Value { get; set; }
    public string Text { get; set; } = string.Empty;
}
