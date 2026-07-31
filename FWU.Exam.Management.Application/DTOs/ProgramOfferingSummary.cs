namespace FWU.Exam.Management.Application.DTOs;

public class ProgramOfferingSummary
{
    public int ProgramId { get; set; }
    public string? ProgramName { get; set; }
    public int SemesterCount { get; set; }
    public int SubjectCount { get; set; }
}
