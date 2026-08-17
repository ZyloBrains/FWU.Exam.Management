using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("resultrecords.manage")]
public class PublishResultsController(
    IPublishResultsService publishResultsService,
    IUserContext userContext,
    AppDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Preview(int examScheduleId, int? collegeId)
    {
        var schedule = await context.ExamSchedules
            .AsNoTracking()
            .Include(s => s.SemesterInstance)
            .ThenInclude(s => s.AcademicYear)
            .Include(s => s.Program)
            .Include(s => s.ExamType)
            .FirstOrDefaultAsync(s => s.Id == examScheduleId);

        if (schedule == null) return NotFound();

        var colleges = await GetCollegesForScheduleAsync(examScheduleId);

        if (colleges.Count == 0)
        {
            TempData["ErrorMessage"] = "No colleges have registered students for this exam schedule.";
            return RedirectToAction("Details", "ExamSchedules", new { id = examScheduleId });
        }

        if (!collegeId.HasValue && colleges.Count == 1)
            collegeId = int.Parse(colleges[0].Value!);

        ViewBag.Colleges = new SelectList(colleges, "Value", "Text", collegeId?.ToString());
        ViewBag.ExamScheduleId = examScheduleId;
        ViewBag.ScheduleName = schedule.ExamScheduleName;
        ViewBag.ProgramName = schedule.Program?.ProgramName;
        ViewBag.SemesterName = schedule.SemesterInstance?.Semester.Name;
        ViewBag.AcademicYearName = schedule.SemesterInstance.AcademicYear?.AcademicYearName;
        ViewBag.ExamTypeName = schedule.ExamType?.Name;

        if (!collegeId.HasValue)
            return View(new Application.DTOs.PublishResultsPreviewDto { ExamScheduleId = examScheduleId });

        var preview = await publishResultsService.GetPreviewAsync(examScheduleId, collegeId.Value);
        return View(preview ?? new Application.DTOs.PublishResultsPreviewDto { ExamScheduleId = examScheduleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int examScheduleId, int collegeId)
    {
        var userId = userContext.UserId;
        var result = await publishResultsService.PublishResultsAsync(examScheduleId, collegeId, userId ?? "system");

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToAction("Preview", new { examScheduleId, collegeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoadPreview(int examScheduleId, int collegeId)
    {
        return RedirectToAction("Preview", new { examScheduleId, collegeId });
    }

    private async Task<List<SelectListItem>> GetCollegesForScheduleAsync(int examScheduleId)
    {
        var query = context.ExamRegistrations
            .AsNoTracking()
            .Where(r => r.ExamScheduleId == examScheduleId && r.IsActive);

        if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
            query = query.Where(r => r.CollegeId == userContext.CollegeId.Value);

        return await query
            .Select(r => r.College!)
            .Where(c => c != null)
            .Distinct()
            .OrderBy(c => c!.Name)
            .Select(c => new SelectListItem { Value = c!.Id.ToString(), Text = c.Name ?? $"College {c.Id}" })
            .ToListAsync();
    }
}
