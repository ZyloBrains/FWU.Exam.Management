namespace FWU.Exam.Management.Application.DTOs;

public class InternalMarksStudentDto
{
    public int ExamRegistrationId { get; set; }
    public int? ExamSubjectResultId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string SymbolNumber { get; set; } = string.Empty;
    public float? TheoryInternal { get; set; }
    public float? PracticalInternal { get; set; }
}

public class InternalMarksSearchResultDto
{
    public int LevelId { get; set; }
    public int ProgramId { get; set; }
    public int SemesterId { get; set; }
    public int ExamTypeId { get; set; }
    public int SubjectOfferingId { get; set; }
    public int ExamScheduleId { get; set; }
    public int AcademicYearId { get; set; }
    public string AcademicYearDisplay { get; set; } = string.Empty;
    public string ExamScheduleDisplay { get; set; } = string.Empty;
    public string CollegeDisplay { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public bool HasPractical { get; set; }
    public bool HasInternal { get; set; }
    public float InternalTheoryFullMarks { get; set; }
    public float InternalTheoryPassMarks { get; set; }
    public float InternalPracticalFullMarks { get; set; }
    public float InternalPracticalPassMarks { get; set; }
    public List<InternalMarksStudentDto> Students { get; set; } = [];
}

public class InternalMarksSaveDto
{
    public int SubjectOfferingId { get; set; }
    public int ExamScheduleId { get; set; }
    public bool SubmitAll { get; set; }
    public List<InternalMarksStudentSaveDto> Students { get; set; } = [];
}

public class InternalMarksStudentSaveDto
{
    public int ExamRegistrationId { get; set; }
    public int? ExamSubjectResultId { get; set; }
    public float? TheoryInternal { get; set; }
    public float? PracticalInternal { get; set; }
}
