using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Application.DTOs;

public class SemesterEnrollmentListItemDto
{
    public int Id { get; set; }
    public string? StudentName { get; set; }
    public string? CollegeRollNumber { get; set; }
    public string? ProgramName { get; set; }
    public string? CollegeName { get; set; }
    public string? SemesterName { get; set; }
    public string? AcademicYearName { get; set; }
    public StudentEnrollmentStatus EnrollmentStatus { get; set; }
    public EnrollmentType EnrollmentType { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public ResultStatus ResultStatus { get; set; }
    public double TotalFee { get; set; }
    public double TotalCredits { get; set; }
}
