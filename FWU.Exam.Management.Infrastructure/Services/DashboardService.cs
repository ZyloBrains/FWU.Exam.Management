using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class DashboardService(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager) : IDashboardService
{
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var tasks = (
            TotalFaculties: context.Faculties.CountAsync(),
            TotalUsers: userManager.Users.CountAsync(),
            TotalRoles: roleManager.Roles.CountAsync(),
            TotalColleges: context.Colleges.CountAsync(),
            TotalPrograms: context.Programs.CountAsync(),
            TotalStudents: context.StudentRegistrations.CountAsync(),
            TotalExamSchedules: context.ExamSchedules.CountAsync(),
            TotalExamRegistrations: context.ExamRegistrations.CountAsync(),
            TotalSubjects: context.SubjectCatalogs.CountAsync(),
            TotalAcademicYears: context.AcademicYears.CountAsync(),
            TotalBanks: context.Banks.CountAsync(),
            TotalBoards: context.Boards.CountAsync(),
            TotalBatches: context.Batches.CountAsync(),
            ActiveColleges: context.Colleges.CountAsync(c => c.IsActive),
            ActivePrograms: context.Programs.CountAsync(p => p.IsActive),
            ActiveStudents: context.StudentRegistrations.CountAsync(s => s.IsActive),
            ActiveExamSchedules: context.ExamSchedules.CountAsync(e => e.IsActive)
        );

        await Task.WhenAll(
            tasks.TotalFaculties, tasks.TotalUsers, tasks.TotalRoles,
            tasks.TotalColleges, tasks.TotalPrograms, tasks.TotalStudents,
            tasks.TotalExamSchedules, tasks.TotalExamRegistrations, tasks.TotalSubjects,
            tasks.TotalAcademicYears, tasks.TotalBanks, tasks.TotalBoards,
            tasks.TotalBatches, tasks.ActiveColleges, tasks.ActivePrograms,
            tasks.ActiveStudents, tasks.ActiveExamSchedules);

        return new DashboardStats
        {
            TotalFaculties = tasks.TotalFaculties.Result,
            TotalUsers = tasks.TotalUsers.Result,
            TotalRoles = tasks.TotalRoles.Result,
            TotalColleges = tasks.TotalColleges.Result,
            TotalPrograms = tasks.TotalPrograms.Result,
            TotalStudents = tasks.TotalStudents.Result,
            TotalExamSchedules = tasks.TotalExamSchedules.Result,
            TotalExamRegistrations = tasks.TotalExamRegistrations.Result,
            TotalSubjects = tasks.TotalSubjects.Result,
            TotalAcademicYears = tasks.TotalAcademicYears.Result,
            TotalBanks = tasks.TotalBanks.Result,
            TotalBoards = tasks.TotalBoards.Result,
            TotalBatches = tasks.TotalBatches.Result,
            ActiveColleges = tasks.ActiveColleges.Result,
            ActivePrograms = tasks.ActivePrograms.Result,
            ActiveStudents = tasks.ActiveStudents.Result,
            ActiveExamSchedules = tasks.ActiveExamSchedules.Result
        };
    }

    public async Task<DashboardStats> GetFacultyDashboardStatsAsync(int facultyId)
    {
        var collegeIdsTask = context.Colleges
            .Where(c => c.FacultyId == facultyId)
            .Select(c => c.Id)
            .ToListAsync();

        var facultyUserIdsTask = userManager.Users
            .Where(u => u.FacultyId == facultyId)
            .Select(u => u.Id)
            .ToListAsync();

        var independentTasks = (
            TotalRoles: roleManager.Roles.CountAsync(),
            TotalPrograms: context.Programs.CountAsync(),
            TotalExamSchedules: context.ExamSchedules.CountAsync(),
            TotalSubjects: context.SubjectCatalogs.CountAsync(),
            TotalAcademicYears: context.AcademicYears.CountAsync(),
            TotalBanks: context.Banks.CountAsync(),
            TotalBoards: context.Boards.CountAsync(),
            TotalBatches: context.Batches.CountAsync(),
            ActivePrograms: context.Programs.CountAsync(p => p.IsActive),
            ActiveExamSchedules: context.ExamSchedules.CountAsync(e => e.IsActive),
            TotalColleges: context.Colleges.CountAsync(c => c.FacultyId == facultyId),
            ActiveColleges: context.Colleges.CountAsync(c => c.FacultyId == facultyId && c.IsActive)
        );

        await Task.WhenAll(
            collegeIdsTask, facultyUserIdsTask,
            independentTasks.TotalRoles, independentTasks.TotalPrograms,
            independentTasks.TotalExamSchedules, independentTasks.TotalSubjects,
            independentTasks.TotalAcademicYears, independentTasks.TotalBanks,
            independentTasks.TotalBoards, independentTasks.TotalBatches,
            independentTasks.ActivePrograms, independentTasks.ActiveExamSchedules,
            independentTasks.TotalColleges, independentTasks.ActiveColleges);

        var collegeIds = collegeIdsTask.Result;
        var facultyUserIds = facultyUserIdsTask.Result;

        return new DashboardStats
        {
            TotalFaculties = 1,
            TotalUsers = facultyUserIds.Count,
            TotalRoles = independentTasks.TotalRoles.Result,
            TotalColleges = independentTasks.TotalColleges.Result,
            TotalPrograms = independentTasks.TotalPrograms.Result,
            TotalStudents = await context.StudentRegistrations.CountAsync(s => collegeIds.Contains(s.CollegeId)),
            TotalExamSchedules = independentTasks.TotalExamSchedules.Result,
            TotalExamRegistrations = await context.ExamRegistrations.CountAsync(e => collegeIds.Contains(e.CollegeId)),
            TotalSubjects = independentTasks.TotalSubjects.Result,
            TotalAcademicYears = independentTasks.TotalAcademicYears.Result,
            TotalBanks = independentTasks.TotalBanks.Result,
            TotalBoards = independentTasks.TotalBoards.Result,
            TotalBatches = independentTasks.TotalBatches.Result,
            ActiveColleges = independentTasks.ActiveColleges.Result,
            ActivePrograms = independentTasks.ActivePrograms.Result,
            ActiveStudents = await context.StudentRegistrations.CountAsync(s => collegeIds.Contains(s.CollegeId) && s.IsActive),
            ActiveExamSchedules = independentTasks.ActiveExamSchedules.Result
        };
    }
}
