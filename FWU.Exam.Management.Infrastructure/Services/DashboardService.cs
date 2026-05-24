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
        return new DashboardStats
        {
            TotalTenants = await context.Tenants.CountAsync(),
            TotalUsers = await userManager.Users.CountAsync(),
            TotalRoles = await roleManager.Roles.CountAsync(),
            TotalColleges = await context.Colleges.CountAsync(),
            TotalPrograms = await context.Programs.CountAsync(),
            TotalStudents = await context.StudentRegistrations.CountAsync(),
            TotalExamSchedules = await context.ExamSchedules.CountAsync(),
            TotalExamRegistrations = await context.ExamRegistrations.CountAsync(),
            TotalSubjects = await context.SubjectCatalogs.CountAsync(),
            TotalAcademicYears = await context.AcademicYears.CountAsync(),
            TotalBanks = await context.Banks.CountAsync(),
            TotalBoards = await context.Boards.CountAsync(),
            TotalBatches = await context.Batches.CountAsync(),
            ActiveColleges = await context.Colleges.CountAsync(c => c.IsActive),
            ActivePrograms = await context.Programs.CountAsync(p => p.IsActive),
            ActiveStudents = await context.StudentRegistrations.CountAsync(s => s.IsActive),
            ActiveExamSchedules = await context.ExamSchedules.CountAsync(e => e.IsActive)
        };
    }
}
