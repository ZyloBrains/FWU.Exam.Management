using FWU.Exam.Management.Domain.Entities.Colleges;

namespace FWU.Exam.Management.Application.DTOs;

/// <summary>
/// One college together with every one of its affiliated programs.
/// Pagination happens over colleges, never over individual CollegeProgram rows,
/// so a college's programs are never split across pages.
/// </summary>
public class CollegeProgramGroupDto
{
    public int CollegeId { get; set; }
    public string? CollegeName { get; set; }
    public string? CollegeCode { get; set; }
    public int ProgramCount { get; set; }
    public int ActiveProgramCount { get; set; }
    public int TotalStudents { get; set; }
    public List<CollegeProgram> Programs { get; set; } = [];
}