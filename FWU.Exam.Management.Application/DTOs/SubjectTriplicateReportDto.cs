namespace FWU.Exam.Management.Application.DTOs;

public class SubjectTriplicateReportDto
{
    public string? ExamScheduleName { get; set; }
    public string? ExamScheduleCode { get; set; }
    public string? AcademicYearName { get; set; }
    public string? LevelName { get; set; }
    public string? SemesterName { get; set; }
    public string? ExamTypeName { get; set; }
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

    public List<SubjectTriplicateCollegeGroupDto> Groups { get; set; } = [];

    public string? CollegeName => Groups.Count switch
    {
        0 => null,
        1 => Groups[0].CollegeName,
        _ => $"{Groups.Count} Colleges"
    };

    public int TotalStudents => Groups.Sum(g => g.Students.Count);

    public List<SubjectSummaryEntry> SubjectSummary =>
        Groups
            .SelectMany(g => g.Students)
            .SelectMany(s => s.Subjects)
            .GroupBy(s => new { s.SubjectCode, s.SubjectName })
            .Select(g => new SubjectSummaryEntry
            {
                SubjectCode = g.Key.SubjectCode,
                SubjectName = g.Key.SubjectName,
                StudentCount = g.Count()
            })
            .OrderBy(s => s.SubjectCode)
            .ToList();

    public int TotalSubjects => SubjectSummary.Count;
}

public class SubjectSummaryEntry
{
    public string? SubjectCode { get; set; }
    public string? SubjectName { get; set; }
    public int StudentCount { get; set; }
}

public class SubjectTriplicateCollegeGroupDto
{
    public int CollegeId { get; set; }
    public string? CollegeName { get; set; }
    public List<SubjectTriplicateStudentDto> Students { get; set; } = [];

    public int TotalStudents => Students.Count;
}

public class SubjectTriplicateStudentDto
{
    public string? SymbolNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? StudentName { get; set; }
    public string? ProgramName { get; set; }
    public List<SubjectTriplicateSubjectDto> Subjects { get; set; } = [];

    public int SubjectCount => Subjects.Count;
    public string SubjectsDisplay => string.Join(", ", Subjects.Select(s => s.Display));
}

public class SubjectTriplicateSubjectDto
{
    public string? SubjectCode { get; set; }
    public string? SubjectName { get; set; }
    public bool HasTheory { get; set; }
    public bool HasPractical { get; set; }

    public string Display
    {
        get
        {
            var legs = (HasTheory, HasPractical) switch
            {
                (true, true) => "T+P",
                (true, false) => "T",
                (false, true) => "P",
                _ => string.Empty
            };
            var label = string.IsNullOrWhiteSpace(SubjectName) ? SubjectCode ?? "" : $"{SubjectCode} - {SubjectName}";
            return string.IsNullOrEmpty(legs) ? label : $"{label} ({legs})";
        }
    }
}
