namespace FWU.Exam.Management.Application.DTOs;

public class GradeResult
{
    public string GradeLetter { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public bool IsPass { get; set; }
    public string? Remark { get; set; }
}
