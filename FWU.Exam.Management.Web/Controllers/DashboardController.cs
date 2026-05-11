using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Controllers;

[Authorize]
public class DashboardController(IDashboardService dashboardService, UserManager<AppUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var roles = await userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Student";

        var stats = await dashboardService.GetDashboardStatsAsync();

        var vm = new DashboardViewModel
        {
            CurrentRole = primaryRole,
            UserName = user.UserName ?? user.Email ?? "User",
            TotalOrganizations = stats.TotalOrganizations,
            TotalUsers = stats.TotalUsers,
            TotalRoles = stats.TotalRoles,
            TotalColleges = stats.TotalColleges,
            TotalPrograms = stats.TotalPrograms,
            TotalStudents = stats.TotalStudents,
            TotalExamSchedules = stats.TotalExamSchedules,
            TotalExamRegistrations = stats.TotalExamRegistrations,
            TotalSubjects = stats.TotalSubjects,
            TotalAcademicYears = stats.TotalAcademicYears,
            TotalBanks = stats.TotalBanks,
            TotalBoards = stats.TotalBoards,
            TotalBatches = stats.TotalBatches,
            ActiveColleges = stats.ActiveColleges,
            ActivePrograms = stats.ActivePrograms,
            ActiveStudents = stats.ActiveStudents,
            ActiveExamSchedules = stats.ActiveExamSchedules
        };

        return primaryRole switch
        {
            "SystemAdmin" => View("SystemAdmin", vm),
            "Admin" => View("Admin", vm),
            "ReportAdmin" => View("ReportAdmin", vm),
            "Student" => View("Student", vm),
            _ => View("Student", vm)
        };
    }
}
