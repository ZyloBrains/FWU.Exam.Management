using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Application.DTOs;

public class ExamRegistrationGroupedDto
{
    public int Id { get; set; }
    public int ExamScheduleId { get; set; }
    public string? ExamScheduleName { get; set; }
    public int CollegeId { get; set; }
    public string? CollegeName { get; set; }
    public int AcademicYearId { get; set; }
    public string? AcademicYearName { get; set; }
    public int? ProgramsId { get; set; }
    public string? ProgramName { get; set; }
    public string? ExamRollNumber { get; set; }
    public string? SymbolNumber { get; set; }
    public string? StudentName { get; set; }
    public decimal? FeeEnclosed { get; set; }
    public RegistrationStatus Status { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public bool IsActive { get; set; }
    public List<ExamSubjectResult> SubjectResults { get; set; } = [];
}
