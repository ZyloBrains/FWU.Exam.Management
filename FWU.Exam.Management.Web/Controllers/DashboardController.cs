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
public class DashboardController(IDashboardService dashboardService, IStudentDashboardService studentDashboardService, UserManager<AppUser> userManager, AppDbContext context) : Controller
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

        if (primaryRole == "Student")
        {
            await PopulateStudentData(vm, user);
        }

        return primaryRole switch
        {
            "SuperAdmin" or "SystemAdmin" => View("SuperAdmin", vm),
            "FacultyAdmin" => View("FacultyAdmin", vm),
            "CollegeAdmin" or "Admin" => View("CollegeAdmin", vm),
            "Student" => View("Student", vm),
            _ => View("Student", vm)
        };
    }

    private async Task PopulateStudentData(DashboardViewModel vm, AppUser user)
    {
        var registration = await studentDashboardService.GetStudentRegistrationByEmailAsync(user.Email!);
        if (registration == null) return;

        vm.StudentName = $"{registration.FirstName} {registration.MiddleName} {registration.LastName}";
        vm.RegistrationNumber = registration.RegistrationNumber;
        vm.CollegeName = registration.College?.Name;
        vm.AcademicYearName = registration.AcademicYear?.AcademicYearName;
        vm.LevelName = registration.Level?.LevelName;

        var admission = await studentDashboardService.GetStudentAdmissionByUserIdAsync(user.Id);
        int programId;
        if (admission != null)
        {
            programId = admission.ProgramsId;
        }
        else if (registration.ProgramId.HasValue)
        {
            programId = registration.ProgramId.Value;
        }
        else
        {
            return;
        }

        var program = await context.Programs.FindAsync(programId);
        vm.StudentProgramName = program?.ProgramName;
        vm.StudentProgramCode = program?.ProgramCode;

        var examSchedules = await studentDashboardService.GetExamSchedulesForStudentAsync(registration, user.Id);
        vm.ExamSchedules = examSchedules;

        var allSubjectOfferings = await studentDashboardService.GetSubjectOfferingsByProgramAsync(programId);

        var latestSchedule = examSchedules.OrderByDescending(es => es.StartDateBs).FirstOrDefault();
        var currentSemesterId = latestSchedule?.SemesterId
            ?? allSubjectOfferings.FirstOrDefault(so => so.Semester != null)?.SemesterId;

        vm.SemesterName = latestSchedule?.Semester?.Name
            ?? allSubjectOfferings.FirstOrDefault(so => so.Semester != null)?.Semester?.Name;

        vm.SubjectOfferings = currentSemesterId.HasValue
            ? allSubjectOfferings.Where(so => so.SemesterId == currentSemesterId.Value).ToList()
            : allSubjectOfferings;
    }
}
