using FWU.Exam.Management.Application.Helpers;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Admin.Controllers;

[Area("Admin")]
[RequirePermission("users.edit")]
public class CollegeAdminAssignmentsController(
    ICollegeAdminSubjectAssignmentService assignmentService,
    UserManager<AppUser> userManager,
    IUserContext userContext,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var items = await assignmentService.GetAssignmentsAsync();
        var collegeAdminIds = items.Select(i => i.CollegeAdminUserId).Distinct().ToList();
        var collegeAdmins = await context.Users
            .Where(u => collegeAdminIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.Email ?? u.UserName ?? u.Id);

        ViewBag.CollegeAdminNames = collegeAdmins;
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new CollegeAdminSubjectAssignment());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CollegeAdminSubjectAssignment model)
    {
        if (ModelState.IsValid)
        {
            model.IsActive = true;
            await assignmentService.CreateAsync(model);
            TempData["Success"] = "College admin assignment created successfully.";
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
    public async Task<IActionResult> Edit(int id, CollegeAdminSubjectAssignment model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            await assignmentService.UpdateAsync(model);
            TempData["Success"] = "College admin assignment updated successfully.";
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
        TempData["Success"] = "College admin assignment deleted successfully.";
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

    private async Task PopulateDropdowns(CollegeAdminSubjectAssignment? model = null)
    {
        var collegeAdmins = await userManager.GetUsersInRoleAsync(Role.CollegeAdmin);
        ViewBag.CollegeAdminUserId = new SelectList(collegeAdmins.Select(t => new { t.Id, Name = t.FullName ?? t.Email }), "Id", "Name", model?.CollegeAdminUserId);

        var programsQuery = context.Programs.AsNoTracking();
        if (userContext.FacultyId.HasValue)
            programsQuery = programsQuery.Where(p => p.FacultyId == userContext.FacultyId.Value);
        ViewBag.ProgramId = new SelectList(await programsQuery.ToListAsync(), "Id", "ProgramName");

        var semesters = await context.Semesters.AsNoTracking().Include(s => s.AcademicYear).ApplyScope(userContext).ToListAsync();
        ViewBag.SemesterId = new SelectList(
            semesters.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = SemesterDisplayHelper.Format(s) }),
            "Value", "Text");

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

        var examSchedulesQuery = context.ExamSchedules.AsNoTracking().Where(es => es.IsActive);
        if (userContext.FacultyId.HasValue)
            examSchedulesQuery = examSchedulesQuery.Where(es => es.Program != null && es.Program.FacultyId == userContext.FacultyId.Value);
        ViewBag.ExamScheduleId = new SelectList(await examSchedulesQuery.ToListAsync(), "Id", "ExamScheduleName", model?.ExamScheduleId);
    }
}
