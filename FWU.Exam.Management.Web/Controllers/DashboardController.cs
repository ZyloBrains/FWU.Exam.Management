using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models;
using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Payments;
using fwu_examination_management_system.Data.Models.Students;
using fwu_examination_management_system.Data.Models.Subjects;
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
        var primaryRole = roles.Contains("SystemAdmin") ? "SystemAdmin"
            : roles.Contains("Admin") ? "Admin"
            : roles.Contains("CollegeAdmin") ? "CollegeAdmin"
            : roles.Contains("ReportAdmin") ? "ReportAdmin"
            : "Student";

        var vm = await BuildDashboardViewModel(user, primaryRole);

        return primaryRole switch
        {
            "SystemAdmin" => View("SystemAdmin", vm),
            "Admin" => View("Admin", vm),
            "CollegeAdmin" => View("CollegeAdmin", vm),
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
            TotalOrganizations = await _context.Set<Organization>().CountAsync(),
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalRoles = await _roleManager.Roles.CountAsync(),
            TotalColleges = await _context.Set<College>().CountAsync(),
            TotalPrograms = await _context.Set<Program>().CountAsync(),
            TotalStudents = await _context.Set<StudentRegistration>().CountAsync(),
            TotalExamSchedules = await _context.Set<ExamSchedule>().CountAsync(),
            TotalExamRegistrations = await _context.Set<ExamRegistration>().CountAsync(),
            TotalSubjects = await _context.Set<SubjectDetail>().CountAsync(),
            TotalAcademicYears = await _context.Set<AcademicYear>().CountAsync(),
            TotalBanks = await _context.Set<Bank>().CountAsync(),
            TotalBoards = await _context.Set<Board>().CountAsync(),
            TotalBatches = await _context.Set<Batch>().CountAsync(),
            ActiveColleges = await _context.Set<College>().CountAsync(c => c.IsActive),
            ActivePrograms = await _context.Set<Program>().CountAsync(p => p.IsActive),
            ActiveStudents = await _context.Set<StudentRegistration>().CountAsync(s => s.IsActive),
            ActiveExamSchedules = await _context.Set<ExamSchedule>().CountAsync(e => e.IsActive)
        };

        if (string.Equals(role, "CollegeAdmin", StringComparison.OrdinalIgnoreCase) && user.CollegeId != null)
        {
            var college = await _context.Set<College>()
                .Include(c => c.Organization)
                .Include(c => c.District)
                .Include(c => c.CollegeType)
                .FirstOrDefaultAsync(c => c.Id == user.CollegeId.Value);

            if (college != null)
            {
                vm.CollegeId = college.Id;
                vm.CollegeName = college.Name;
                vm.CollegeCode = college.Code;
                vm.CollegeAddress = college.Address ?? string.Empty;
                vm.CollegeDistrict = !string.IsNullOrWhiteSpace(college.DistrictName)
                    ? college.DistrictName
                    : college.District?.DistrictName ?? string.Empty;
                vm.CollegeType = !string.IsNullOrWhiteSpace(college.CollegeTypeName)
                    ? college.CollegeTypeName
                    : college.CollegeType?.Name ?? string.Empty;
                vm.CollegeOrganization = college.Organization?.Name ?? string.Empty;
                vm.CollegeUsersCount = await _userManager.Users.CountAsync(u => u.CollegeId == college.Id);
                vm.CollegeProgramsCount = await _context.Set<CollegeProgram>().CountAsync(cp => cp.CollegeId == college.Id);
                vm.CollegeStudentsCount = await _context.Set<StudentRegistration>().CountAsync(sr => sr.CollegeId == college.Id);
                vm.CollegeExamRegistrationsCount = await _context.Set<ExamRegistration>().CountAsync(er => er.CollegeId == college.Id);
                vm.CollegeExamCentersCount = await _context.Set<ExamCenter>().CountAsync(ec => ec.CollegeId == college.Id);
            }
        }

        return vm;
    }
}
