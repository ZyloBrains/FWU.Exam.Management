namespace FWU.Exam.Management.Application.DTOs;

public class SemesterEnrollmentCandidateDto
{
    public int AdmissionId { get; set; }
    public string? StudentName { get; set; }
    public string? CollegeRollNumber { get; set; }
    public string? ProgramName { get; set; }
    public string? CollegeName { get; set; }
    public string? AcademicYearName { get; set; }
    public bool IsEnrolled { get; set; }
}
