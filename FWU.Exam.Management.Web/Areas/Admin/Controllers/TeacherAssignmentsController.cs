using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Teachers;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Admin.Controllers;

[Area("Admin")]
[RequirePermission("users.edit")]
public class TeacherAssignmentsController(
    ITeacherSubjectAssignmentService assignmentService,
    UserManager<AppUser> userManager,
    AppDbContext context) : Controller
{
    private async Task<int?> GetCurrentUserFacultyIdAsync()
    {
        var user = await userManager.GetUserAsync(User);
        return user?.FacultyId;
    }

    public async Task<IActionResult> Index()
    {
        int? facultyId = User.IsInRole(Role.FacultyAdmin) ? await GetCurrentUserFacultyIdAsync() : null;
        var items = await assignmentService.GetAssignmentsAsync(facultyId: facultyId);
        var teacherIds = items.Select(i => i.TeacherUserId).Distinct().ToList();
        var teachers = await context.Users
            .Where(u => teacherIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.Email ?? u.UserName ?? u.Id);

        ViewBag.TeacherNames = teachers;
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new TeacherSubjectAssignment());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeacherSubjectAssignment model)
    {
        if (ModelState.IsValid)
        {
            model.IsActive = true;
            await assignmentService.CreateAsync(model);
            TempData["Success"] = "Teacher assignment created successfully.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns(model);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var assignment = await assignmentService.GetByIdAsync(id);
        if (assignment == null) return NotFound();

        await PopulateDropdowns(assignment);
        return View(assignment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TeacherSubjectAssignment model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            await assignmentService.UpdateAsync(model);
            TempData["Success"] = "Teacher assignment updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await assignmentService.DeleteAsync(id);
        TempData["Success"] = "Teacher assignment deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<JsonResult> GetSubjectOfferings(int programId, int semesterId)
    {
        var offerings = await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .Where(so => so.ProgramId == programId && so.SemesterId == semesterId)
            .Select(so => new { so.Id, Name = so.SubjectCatalog!.SubjectName })
            .ToListAsync();

        return Json(offerings);
    }

    private async Task PopulateDropdowns(TeacherSubjectAssignment? model = null)
    {
        var teachers = await userManager.GetUsersInRoleAsync("Teacher");
        ViewBag.TeacherUserId = new SelectList(teachers.Select(t => new { t.Id, Name = t.FullName ?? t.Email }), "Id", "Name", model?.TeacherUserId);

        ViewBag.ProgramId = new SelectList(await context.Programs.AsNoTracking().ToListAsync(), "Id", "ProgramName");
        ViewBag.SemesterId = new SelectList(await context.Semesters.AsNoTracking().ToListAsync(), "Id", "Name");

        if (model?.SubjectOfferingId > 0)
        {
            var so = await context.SubjectOfferings
                .AsNoTracking()
                .Include(s => s.SubjectCatalog)
                .FirstOrDefaultAsync(s => s.Id == model.SubjectOfferingId);
            if (so != null)
            {
                ViewBag.SelectedProgramId = so.ProgramId;
                ViewBag.SelectedSemesterId = so.SemesterId;
            }
        }

        ViewBag.ExamScheduleId = new SelectList(
            await context.ExamSchedules.AsNoTracking().Where(es => es.IsActive).ToListAsync(),
            "Id", "ExamScheduleName", model?.ExamScheduleId);
    }
}
