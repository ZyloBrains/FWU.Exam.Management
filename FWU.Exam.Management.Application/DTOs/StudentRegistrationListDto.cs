namespace FWU.Exam.Management.Application.DTOs;

public class StudentRegistrationListDto
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
