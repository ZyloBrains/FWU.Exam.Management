using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DashboardService(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        return new DashboardStats
        {
            TotalOrganizations = await _context.Organizations.CountAsync(),
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalRoles = await _roleManager.Roles.CountAsync(),
            TotalColleges = await _context.Colleges.CountAsync(),
            TotalPrograms = await _context.Programs.CountAsync(),
            TotalStudents = await _context.StudentRegistrations.CountAsync(),
            TotalExamSchedules = await _context.ExamSchedules.CountAsync(),
            TotalExamRegistrations = await _context.ExamRegistrations.CountAsync(),
            TotalSubjects = await _context.SubjectCatalogs.CountAsync(),
            TotalAcademicYears = await _context.AcademicYears.CountAsync(),
            TotalBanks = await _context.Banks.CountAsync(),
            TotalBoards = await _context.Boards.CountAsync(),
            TotalBatches = await _context.Batches.CountAsync(),
            ActiveColleges = await _context.Colleges.CountAsync(c => c.IsActive),
            ActivePrograms = await _context.Programs.CountAsync(p => p.IsActive),
            ActiveStudents = await _context.StudentRegistrations.CountAsync(s => s.IsActive),
            ActiveExamSchedules = await _context.ExamSchedules.CountAsync(e => e.IsActive)
        };
    }
}
