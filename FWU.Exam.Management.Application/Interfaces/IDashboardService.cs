using System.Threading.Tasks;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync();
    Task<DashboardStats> GetFacultyDashboardStatsAsync(int facultyId);
}

public class DashboardStats
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
}
