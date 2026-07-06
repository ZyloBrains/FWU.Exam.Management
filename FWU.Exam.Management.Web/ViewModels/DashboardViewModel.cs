using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Web.ViewModels;

public class DashboardViewModel
{
    public int TotalFaculties { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRoles { get; set; }
    public int TotalColleges { get; set; }
    public int TotalPrograms { get; set; }
    public int TotalStudents { get; set; }
    public int TotalExamSchedules { get; set; }
    public int TotalExamRegistrations { get; set; }
    public int TotalSubjects { get; set; }
    public int TotalAcademicYears { get; set; }
    public int TotalBanks { get; set; }
    public int TotalBoards { get; set; }
    public int TotalBatches { get; set; }
    public int ActiveColleges { get; set; }
    public int ActivePrograms { get; set; }
    public int ActiveStudents { get; set; }
    public int ActiveExamSchedules { get; set; }
    public string CurrentRole { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public string? StudentName { get; set; }
    public string? StudentProgramName { get; set; }
    public string? StudentProgramCode { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? CollegeName { get; set; }
    public string? AcademicYearName { get; set; }
    public string? LevelName { get; set; }
    public string? SemesterName { get; set; }
    public List<SubjectOffering>? SubjectOfferings { get; set; }
    public List<ExamSchedule>? ExamSchedules { get; set; }
}
