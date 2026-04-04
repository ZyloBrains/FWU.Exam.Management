using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models;
using fwu_examination_management_system.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DashboardController(ApplicationDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Student";

        var vm = await BuildDashboardViewModel(user, primaryRole);

        return primaryRole switch
        {
            "SystemAdmin" => View("SystemAdmin", vm),
            "Admin" => View("Admin", vm),
            "ReportAdmin" => View("ReportAdmin", vm),
            "Student" => View("Student", vm),
            _ => View("Student", vm)
        };
    }

    private async Task<DashboardViewModel> BuildDashboardViewModel(AppUser user, string role)
    {
        var vm = new DashboardViewModel
        {
            CurrentRole = role,
            UserName = user.UserName ?? user.Email ?? "User",
            TotalOrganizations = await _context.Organizations.CountAsync(),
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalRoles = await _roleManager.Roles.CountAsync(),
            TotalColleges = await _context.Colleges.CountAsync(),
            TotalPrograms = await _context.Programs.CountAsync(),
            TotalStudents = await _context.StudentRegistrations.CountAsync(),
            TotalExamSchedules = await _context.ExamSchedules.CountAsync(),
            TotalExamRegistrations = await _context.ExamRegistrations.CountAsync(),
            TotalSubjects = await _context.SubjectDetails.CountAsync(),
            TotalAcademicYears = await _context.AcademicYears.CountAsync(),
            TotalBanks = await _context.Banks.CountAsync(),
            TotalBoards = await _context.Boards.CountAsync(),
            TotalBatches = await _context.Batches.CountAsync(),
            ActiveColleges = await _context.Colleges.CountAsync(c => c.IsActive),
            ActivePrograms = await _context.Programs.CountAsync(p => p.IsActive),
            ActiveStudents = await _context.StudentRegistrations.CountAsync(s => s.IsActive),
            ActiveExamSchedules = await _context.ExamSchedules.CountAsync(e => e.IsActive)
        };

        return vm;
    }
}
