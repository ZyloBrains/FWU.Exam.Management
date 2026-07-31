namespace FWU.Exam.Management.Application.DTOs;

public class SemesterOfferingSummary
{
    public int SemesterId { get; set; }
    public int SemesterNumber { get; set; }
    public string? SemesterName { get; set; }
    public int SubjectCount { get; set; }
}
