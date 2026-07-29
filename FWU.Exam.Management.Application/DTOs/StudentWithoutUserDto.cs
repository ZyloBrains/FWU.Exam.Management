namespace FWU.Exam.Management.Application.DTOs;

public class StudentWithoutUserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? RegistrationNumber { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public string? FacultyName { get; set; }
    public string DateOfBirthBS { get; set; } = string.Empty;
    public bool HasEmail { get; set; }
}
