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

    public async Task<IActionResult> Index()
    {
        var perms = await GetUserPermissions();
        var sections = new List<SectionCardViewModel>();

        if (perms.Any(p => p is "users.view" or "users.create" or "roles.view" or "permissions.manage"))
            sections.Add(new("User Management", "fas fa-users", "text-blue-600", "bg-blue-100", "group-hover:bg-blue-200", "Manage users, roles and permissions", Url.Action("UserManagement")!));

        if (perms.Any(p => p is "faculties.view" or "programs.view" or "semesters.view" or "academicyears.view" or "subjects.view" or "subjecttypes.view" or "subjectofferings.view" or "curriculumversions.view"))
            sections.Add(new("Academic Setup", "fas fa-graduation-cap", "text-green-600", "bg-green-100", "group-hover:bg-green-200", "Configure faculties, programs, semesters and subjects", Url.Action("AcademicSetup")!));

        if (perms.Any(p => p is "colleges.view" or "collegetypes.view" or "collegeprograms.view"))
            sections.Add(new("Colleges", "fas fa-school", "text-purple-600", "bg-purple-100", "group-hover:bg-purple-200", "Manage colleges and their programs", Url.Action("Colleges")!));

        if (perms.Any(p => p is "students.view" or "studentcategories.view" or "studentadmissions.view"))
            sections.Add(new("Registration", "fas fa-address-card", "text-teal-600", "bg-teal-100", "group-hover:bg-teal-200", "Manage student registrations and admissions", Url.Action("Registration")!));

        if (perms.Any(p => p is "examschedules.view" or "examtypes.view" or "entrance.view" or "gradingschemes.view" or "examregistrations.view" or "examcenters.view" or "admitcards.view" or "marksentry.view"))
            sections.Add(new("Examination", "fas fa-file-alt", "text-red-600", "bg-red-100", "group-hover:bg-red-200", "Manage exam schedules, types, registrations and more", Url.Action("Examination")!));

        if (perms.Any(p => p is "examsubjectresults.view" or "resultrecords.view" or "retotaling.view"))
            sections.Add(new("Results", "fas fa-poll", "text-yellow-600", "bg-yellow-100", "group-hover:bg-yellow-200", "View and manage examination results", Url.Action("Results")!));

        if (perms.Any(p => p is "banks.view" or "paymenttypes.view" or "billtitles.view"))
            sections.Add(new("Payments", "fas fa-money-bill-wave", "text-orange-600", "bg-orange-100", "group-hover:bg-orange-200", "Manage banks, payment types and bill titles", Url.Action("Payments")!));

        if (perms.Any(p => p is "provinces.view" or "districts.view" or "locallevels.view"))
            sections.Add(new("Location", "fas fa-map-marker-alt", "text-cyan-600", "bg-cyan-100", "group-hover:bg-cyan-200", "Manage provinces, districts and local levels", Url.Action("Location")!));

        if (perms.Any(p => p is "student.portal.profile" or "student.portal.examforms" or "student.portal.marksheet" or "student.portal.payment" or "retotaling.view"))
            sections.Add(new("Academic (Student Portal)", "fas fa-user-graduate", "text-pink-600", "bg-pink-100", "group-hover:bg-pink-200", "Student portal - profile, exams, marksheet and payments", Url.Action("StudentPortal")!));

        if (perms.Any(p => p is "tenants.view" or "smtp.view" or "sms.view" or "esewa.view" or "khalti.view" or "connectips.view" or "notices.view" or "auditlog.view" or "backuprestore.manage"))
            sections.Add(new("System Config", "fas fa-cog", "text-indigo-600", "bg-indigo-100", "group-hover:bg-indigo-200", "Configure tenants, SMTP, SMS, payment gateways and more", Url.Action("SystemConfig")!));

        if (perms.Any(p => p is "reports.collegepayments" or "reports.subjectcount" or "reports.examtriplicate" or "reports.summary" or "reports.tabulationtriplicate" or "reports.programwisestudent" or "reports.attendanceheet" or "reports.marksfoil" or "reports.bankvoucherlist"))
            sections.Add(new("Reports", "fas fa-chart-line", "text-rose-600", "bg-rose-100", "group-hover:bg-rose-200", "Generate and view various reports", Url.Action("Index", "Reports", new { area = "Reports" })!));

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
