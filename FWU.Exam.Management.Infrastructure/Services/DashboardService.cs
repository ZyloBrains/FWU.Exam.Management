using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class DashboardService(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IUserContext userContext) : IDashboardService
{
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var totalFaculties = await context.Faculties.ApplyScope(userContext).CountAsync();
        var totalUsers = await userManager.Users.ApplyScope(userContext).CountAsync();
        var totalRoles = await roleManager.Roles.CountAsync();
        var totalColleges = await context.Colleges.ApplyScope(userContext).CountAsync();
        var totalPrograms = await context.Programs.ApplyScope(userContext).CountAsync();
        var totalStudents = await context.StudentRegistrations.ApplyScope(userContext).CountAsync();
        var totalExamSchedules = await context.ExamSchedules.ApplyScope(userContext).CountAsync();
        var totalExamRegistrations = await context.ExamRegistrations.ApplyScope(userContext).CountAsync();
        var totalSubjects = await context.SubjectCatalogs.ApplyScope(userContext).CountAsync();
        var totalAcademicYears = await context.AcademicYears.CountAsync();
        var totalBanks = await context.Banks.CountAsync();
        var totalBoards = await context.Boards.CountAsync();
        var totalBatches = await context.Batches.CountAsync();
        var activeColleges = await context.Colleges.ApplyScope(userContext).CountAsync(c => c.IsActive);
        var activePrograms = await context.Programs.ApplyScope(userContext).CountAsync(p => p.IsActive);
        var activeStudents = await context.StudentRegistrations.ApplyScope(userContext).CountAsync(s => s.IsActive);
        var activeExamSchedules = await context.ExamSchedules.ApplyScope(userContext).CountAsync(e => e.IsActive);

        return new DashboardStats
        {
            TotalFaculties = totalFaculties,
            TotalUsers = totalUsers,
            TotalRoles = totalRoles,
            TotalColleges = totalColleges,
            TotalPrograms = totalPrograms,
            TotalStudents = totalStudents,
            TotalExamSchedules = totalExamSchedules,
            TotalExamRegistrations = totalExamRegistrations,
            TotalSubjects = totalSubjects,
            TotalAcademicYears = totalAcademicYears,
            TotalBanks = totalBanks,
            TotalBoards = totalBoards,
            TotalBatches = totalBatches,
            ActiveColleges = activeColleges,
            ActivePrograms = activePrograms,
            ActiveStudents = activeStudents,
            ActiveExamSchedules = activeExamSchedules
        };
    }

    public async Task<DashboardStats> GetFacultyDashboardStatsAsync(int facultyId)
    {
        var tenantCollegeIds = await context.Colleges
            .AsNoTracking()
            .Select(c => c.Id)
            .ToListAsync();

        var collegeIds = (await context.CollegePrograms
            .AsNoTracking()
            .Where(cp => cp.Program != null && cp.Program.FacultyId == facultyId)
            .Select(cp => cp.CollegeId)
            .Distinct()
            .ToListAsync())
            .Intersect(tenantCollegeIds)
            .ToList();

        var facultyUserIds = await userManager.Users
            .Where(u => u.FacultyId == facultyId)
            .Select(u => u.Id)
            .ToListAsync();

        var totalRoles = await roleManager.Roles.CountAsync();
        var totalPrograms = await context.Programs.CountAsync(p => p.FacultyId == facultyId);
        var totalExamSchedules = await context.ExamSchedules.CountAsync(es => es.Program != null && es.Program.FacultyId == facultyId);
        var totalSubjects = await context.SubjectCatalogs.CountAsync(s => s.SubjectOfferings != null && s.SubjectOfferings.Any(so => so.Program != null && so.Program.FacultyId == facultyId));
        var totalAcademicYears = await context.AcademicYears.CountAsync();
        var totalBanks = await context.Banks.CountAsync();
        var totalBoards = await context.Boards.CountAsync();
        var totalBatches = await context.Batches.CountAsync();
        var activePrograms = await context.Programs.CountAsync(p => p.FacultyId == facultyId && p.IsActive);
        var activeExamSchedules = await context.ExamSchedules.CountAsync(es => es.Program != null && es.Program.FacultyId == facultyId && es.IsActive);
        var totalColleges = collegeIds.Count;
        var activeColleges = await context.Colleges.CountAsync(c => collegeIds.Contains(c.Id) && c.IsActive);
        var totalStudents = await context.StudentRegistrations.CountAsync(s => s.FacultyId == facultyId);
        var totalExamRegistrations = await context.ExamRegistrations.CountAsync(e => e.Program != null && e.Program.FacultyId == facultyId);
        var activeStudents = await context.StudentRegistrations.CountAsync(s => s.FacultyId == facultyId && s.IsActive);

        return new DashboardStats
        {
            TotalFaculties = 1,
            TotalUsers = facultyUserIds.Count,
            TotalRoles = totalRoles,
            TotalColleges = totalColleges,
            TotalPrograms = totalPrograms,
            TotalStudents = totalStudents,
            TotalExamSchedules = totalExamSchedules,
            TotalExamRegistrations = totalExamRegistrations,
            TotalSubjects = totalSubjects,
            TotalAcademicYears = totalAcademicYears,
            TotalBanks = totalBanks,
            TotalBoards = totalBoards,
            TotalBatches = totalBatches,
            ActiveColleges = activeColleges,
            ActivePrograms = activePrograms,
            ActiveStudents = activeStudents,
            ActiveExamSchedules = activeExamSchedules
        };
    }

    public async Task<DashboardStats> GetCollegeDashboardStatsAsync(int collegeId)
    {
        var totalRoles = await roleManager.Roles.CountAsync();
        var totalPrograms = await context.Programs.CountAsync(p => p.CollegePrograms!.Any(cp => cp.CollegeId == collegeId));
        var totalExamSchedules = await context.ExamSchedules.CountAsync(e => e.CollegeId == collegeId);
        var totalSubjects = await context.SubjectCatalogs.CountAsync(sc =>
            sc.SubjectOfferings!.Any(so => so.Program != null && so.Program.CollegePrograms!.Any(cp => cp.CollegeId == collegeId)));
        var totalAcademicYears = await context.AcademicYears.CountAsync();
        var activePrograms = await context.Programs.CountAsync(p => p.IsActive && p.CollegePrograms!.Any(cp => cp.CollegeId == collegeId));
        var activeExamSchedules = await context.ExamSchedules.CountAsync(e => e.IsActive && e.CollegeId == collegeId);
        var totalColleges = await context.Colleges.CountAsync(c => c.Id == collegeId);
        var activeColleges = await context.Colleges.CountAsync(c => c.Id == collegeId && c.IsActive);
        var totalStudents = await context.StudentRegistrations.CountAsync(s => s.CollegeId == collegeId);
        var totalExamRegistrations = await context.ExamRegistrations.CountAsync(e => e.CollegeId == collegeId);
        var activeStudents = await context.StudentRegistrations.CountAsync(s => s.CollegeId == collegeId && s.IsActive);
        var totalUsers = await userManager.Users.CountAsync(u => u.CollegeId == collegeId);

        return new DashboardStats
        {
            TotalFaculties = 1,
            TotalUsers = totalUsers,
            TotalRoles = totalRoles,
            TotalColleges = totalColleges,
            TotalPrograms = totalPrograms,
            TotalStudents = totalStudents,
            TotalExamSchedules = totalExamSchedules,
            TotalExamRegistrations = totalExamRegistrations,
            TotalSubjects = totalSubjects,
            TotalAcademicYears = totalAcademicYears,
            ActiveColleges = activeColleges,
            ActivePrograms = activePrograms,
            ActiveStudents = activeStudents,
            ActiveExamSchedules = activeExamSchedules
        };
    }
}
