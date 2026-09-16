namespace FWU.Exam.Management.Application.DTOs;

public class AdmitCardListDto
{
    public int Id { get; set; }
    public string AdmitCardNumber { get; set; } = string.Empty;
    public string ExamScheduleName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public bool IsActive { get; set; }
}