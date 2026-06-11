using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

[Authorize]
public class DashboardController(IDashboardService dashboardService, UserManager<AppUser> userManager, AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var roles = await userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Student";

        if (primaryRole == Role.FacultyAdmin && user.FacultyId != null)
        {
            var faculty = await context.Faculties.FindAsync(user.FacultyId.Value);
            if (faculty?.OfficeCode != null)
                return RedirectToAction("Index", "FacultyDashboard", new { officeCode = faculty.OfficeCode });
        }

        DashboardStats stats;
        if ((primaryRole == "CollegeAdmin" || primaryRole == "Admin") && user.CollegeId.HasValue)
        {
            stats = await dashboardService.GetCollegeDashboardStatsAsync(user.CollegeId.Value);
        }
        else
        {
            stats = await dashboardService.GetDashboardStatsAsync();
        }

        var vm = new DashboardViewModel
        {
            CurrentRole = primaryRole,
            UserName = user.UserName ?? user.Email ?? "User",
            TotalFaculties = stats.TotalFaculties,
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
            "SuperAdmin" or "SystemAdmin" => View("SuperAdmin", vm),
            "FacultyAdmin" => View("FacultyAdmin", vm),
            "CollegeAdmin" or "Admin" => View("CollegeAdmin", vm),
            "Student" => View("Student", vm),
            _ => View("Student", vm)
        };
    }
}
