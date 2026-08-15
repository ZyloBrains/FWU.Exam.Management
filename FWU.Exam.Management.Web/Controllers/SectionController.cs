using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using FWU.Exam.Management.Web.Navigation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Controllers;

public class SectionController(IPermissionService permissionService, UserManager<AppUser> userManager) : Controller
{
    private async Task<List<string>> GetUserPermissions()
    {
        var user = await userManager.GetUserAsync(User);
        return user != null ? await permissionService.GetUserPermissionsAsync(user.Id) : new List<string>();
    }

    private async Task<bool> HasAnyAsync(params string[] permissions)
    {
        var perms = await GetUserPermissions();
        return permissions.Any(p => perms.Contains(p));
    }

    private bool IsSectionVisible(MenuSection section, List<string> perms, bool isSuperAdmin, IList<string> userRoles)
    {
        if (section.HideFromStudents && !isSuperAdmin && userRoles.Contains(Role.Student)) return false;
        if (section.SingleLink) return true;
        if (section.SuperAdminOnly && !isSuperAdmin) return false;
        if (section.RoleGate != null && !isSuperAdmin && !userRoles.Contains(section.RoleGate)) return false;
        return section.Items.Any(i => i.Permissions.Length == 0 || i.Permissions.Any(perms.Contains));
    }

    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        var perms = await GetUserPermissions();
        var isSuperAdmin = user != null && await userManager.IsInRoleAsync(user, Role.SuperAdmin);
        var userRoles = user != null ? await userManager.GetRolesAsync(user) : new List<string>();

        var sections = new List<SectionCardViewModel>();
        foreach (var section in AppMenu.Sections)
        {
            if (!section.ShowOnDashboard) continue;
            if (!IsSectionVisible(section, perms, isSuperAdmin, userRoles)) continue;

            var url = section.Landing != null
                ? Url.Action(section.Landing.Action, section.Landing.Controller, new { area = section.Landing.Area })!
                : "";
            sections.Add(new(section.Title, section.Icon, section.IconColor, section.BgColor, section.HoverBgColor, section.Description, url));
        }

        return View(sections);
    }

    public async Task<IActionResult> UserManagement()
    {
        if (!await HasAnyAsync("users.view", "users.create", "roles.view", "permissions.manage"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> AcademicSetup()
    {
        if (!await HasAnyAsync("faculties.view", "programs.view", "semesters.view", "academicyears.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> Subjects()
    {
        if (!await HasAnyAsync("subjects.view", "subjecttypes.view", "subjectofferings.view", "curriculumversions.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> Colleges()
    {
        if (!await HasAnyAsync("colleges.view", "collegetypes.view", "collegeprograms.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> Registration()
    {
        if (!await HasAnyAsync("students.view", "studentcategories.view", "studentadmissions.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> Examination()
    {
        if (!await HasAnyAsync("examschedules.view", "examtypes.view", "entrance.view",
            "examregistration.view", "admitcards.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> ExamCenters()
    {
        if (!await HasAnyAsync("examcenters.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> GradingAndMarks()
    {
        if (!await HasAnyAsync("gradingschemes.view", "marksentry.view", "theorymarks.view", "practicalmarks.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> Results()
    {
        if (!await HasAnyAsync("examsubjectresults.view", "resultrecords.view", "retotaling.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> Payments()
    {
        if (!await HasAnyAsync("banks.view", "paymenttypes.view", "billtitles.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> Location()
    {
        if (!await HasAnyAsync("provinces.view", "districts.view", "locallevels.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> StudentPortal()
    {
        if (!await HasAnyAsync("student.portal.profile", "student.portal.examforms", "student.portal.marksheet",
            "student.portal.payment", "retotaling.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> SystemConfig()
    {
        if (!await HasAnyAsync("tenants.view", "notices.view", "auditlog.view", "backuprestore.manage"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> EmailAndSms()
    {
        if (!await HasAnyAsync("smtp.view", "sms.view", "gumpnowemail.view"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> PaymentGateways()
    {
        if (!await HasAnyAsync("esewa.view", "khalti.view", "connectips.view"))
            return Forbid();
        return View();
    }
}
