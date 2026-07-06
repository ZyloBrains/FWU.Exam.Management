namespace FWU.Exam.Management.Application.DTOs;

public class TeacherAssignmentListItemDto
{
    public int Id { get; set; }
    public string TeacherUserId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public string? ExamScheduleName { get; set; }
    public bool IsActive { get; set; }
    public int SubjectOfferingId { get; set; }
    public int? ExamScheduleId { get; set; }
}
