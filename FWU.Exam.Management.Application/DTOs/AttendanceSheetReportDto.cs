namespace FWU.Exam.Management.Application.DTOs;

public class AttendanceSheetReportDto
{
    public string? ExamScheduleName { get; set; }
    public string? ExamScheduleCode { get; set; }
    public string? AcademicYearName { get; set; }
    public string? LevelName { get; set; }
    public string? SemesterName { get; set; }
    public string? ExamTypeName { get; set; }
    public string? CollegeName { get; set; }
    public string? ProgramName { get; set; }
    public string? CenterName { get; set; }
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

    public List<AttendanceSheetStudentDto> Students { get; set; } = [];

    public int TotalStudents => Students.Count;
}

public class AttendanceSheetStudentDto
{
    public string? RollNo { get; set; }
    public string? StudentName { get; set; }
}