using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
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

    public async Task<IActionResult> UserManagement()
    {
        if (!await HasAnyAsync("users.view", "users.create", "roles.view", "permissions.manage"))
            return Forbid();
        return View();
    }

    public async Task<IActionResult> AcademicSetup()
    {
        if (!await HasAnyAsync("faculties.view", "programs.view", "semesters.view", "academicyears.view",
            "subjects.view", "subjecttypes.view", "subjectofferings.view", "curriculumversions.view"))
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
        if (!await HasAnyAsync("examschedules.view", "examtypes.view", "entrance.view", "gradingschemes.view",
            "examregistrations.view", "examcenters.view", "admitcards.view", "marksentry.view"))
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
        if (!await HasAnyAsync("tenants.view", "smtp.view", "sms.view", "esewa.view", "khalti.view",
            "connectips.view", "notices.view", "auditlog.view", "backuprestore.manage"))
            return Forbid();
        return View();
    }
}
