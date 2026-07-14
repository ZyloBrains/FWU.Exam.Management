namespace FWU.Exam.Management.Application.DTOs;

public class CollegeAdminAssignmentListItemDto
{
    public int Id { get; set; }
    public string CollegeAdminUserId { get; set; } = string.Empty;
    public string CollegeAdminName { get; set; } = string.Empty;
    public string CollegeAdminEmail { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public string? ExamScheduleName { get; set; }
    public bool IsActive { get; set; }
    public int SubjectOfferingId { get; set; }
    public int? ExamScheduleId { get; set; }
}
