using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("marksentry.view")]
public class CollegeAdminMarksController(
    ICollegeAdminMarksService collegeAdminMarksService,
    ICollegeAdminSubjectAssignmentService assignmentService,
    UserManager<AppUser> userManager,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var dashboard = await collegeAdminMarksService.GetCollegeAdminDashboardAsync(user.Id);
        return View(dashboard);
    }

    public async Task<IActionResult> MarksEntry(int subjectOfferingId, int examScheduleId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        try
        {
            var model = await collegeAdminMarksService.GetMarksEntryViewAsync(subjectOfferingId, examScheduleId, user.Id);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [RequirePermission("marksentry.submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarksEntry([FromForm] BulkMarksSaveDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        try
        {
            var result = await collegeAdminMarksService.SaveMarksBulkAsync(dto, user.Id);

            if (result.Success)
            {
                TempData["Success"] = $"{result.SavedCount} student marks saved successfully.";
            }
            else
            {
                TempData["Error"] = $"Saved {result.SavedCount} records with {result.Errors.Count} errors: {string.Join("; ", result.Errors)}";
            }

            return RedirectToAction(nameof(MarksEntry), new { subjectOfferingId = dto.SubjectOfferingId, examScheduleId = dto.ExamScheduleId });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    public async Task<IActionResult> MarksEntryExcel(int subjectOfferingId, int examScheduleId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        try
        {
            var model = await collegeAdminMarksService.GetMarksEntryViewAsync(subjectOfferingId, examScheduleId, user.Id);
            return View("MarksEntryExcel", model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [RequirePermission("marksentry.submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarksEntryExcel([FromForm] BulkMarksSaveDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        try
        {
            var result = await collegeAdminMarksService.SaveMarksBulkAsync(dto, user.Id);

            if (result.Success)
            {
                TempData["Success"] = $"{result.SavedCount} student marks saved successfully.";
            }
            else
            {
                TempData["Error"] = $"Saved {result.SavedCount} records with {result.Errors.Count} errors: {string.Join("; ", result.Errors)}";
            }

            return RedirectToAction(nameof(MarksEntryExcel), new { subjectOfferingId = dto.SubjectOfferingId, examScheduleId = dto.ExamScheduleId });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [RequirePermission("marksentry.export")]
    public async Task<IActionResult> ExportMarks(int subjectOfferingId, int examScheduleId)
    {
        try
        {
            var data = await collegeAdminMarksService.ExportMarksAsync(subjectOfferingId, examScheduleId);
            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Marks_S{subjectOfferingId}_E{examScheduleId}.xlsx");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [RequirePermission("marksentry.export")]
    public async Task<IActionResult> ExportTemplate(int subjectOfferingId, int examScheduleId)
    {
        try
        {
            var data = await collegeAdminMarksService.ExportMarksTemplateAsync(subjectOfferingId, examScheduleId);
            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"MarksTemplate_S{subjectOfferingId}_E{examScheduleId}.xlsx");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [RequirePermission("marksentry.import")]
    public async Task<IActionResult> Import(int subjectOfferingId, int examScheduleId, string source = "standard")
    {
        ViewBag.SubjectOfferingId = subjectOfferingId;
        ViewBag.ExamScheduleId = examScheduleId;
        ViewBag.Source = source;
        return View();
    }

    [HttpPost]
    [RequirePermission("marksentry.import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile excelFile, int subjectOfferingId, int examScheduleId, string source = "standard")
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (excelFile == null || excelFile.Length == 0)
        {
            ModelState.AddModelError("excelFile", "Please select an Excel file.");
            ViewBag.SubjectOfferingId = subjectOfferingId;
            ViewBag.ExamScheduleId = examScheduleId;
            ViewBag.Source = source;
            return View();
        }

        try
        {
            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            stream.Position = 0;

            var result = await collegeAdminMarksService.ImportMarksFromExcelAsync(stream, subjectOfferingId, examScheduleId, user.Id);

            if (result.ErrorCount == 0)
            {
                TempData["Success"] = $"{result.SuccessCount} marks imported successfully.";
            }
            else
            {
                TempData["Error"] = $"Imported {result.SuccessCount} with {result.ErrorCount} errors: {string.Join("; ", result.Errors)}";
            }

            if (source == "excel")
                return RedirectToAction(nameof(MarksEntryExcel), new { subjectOfferingId, examScheduleId });
            return RedirectToAction(nameof(MarksEntry), new { subjectOfferingId, examScheduleId });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Import failed: {ex.Message}");
            ViewBag.SubjectOfferingId = subjectOfferingId;
            ViewBag.ExamScheduleId = examScheduleId;
            ViewBag.Source = source;
            return View();
        }
    }

    [HttpGet]
    public async Task<JsonResult> GetAssignedSchedules(int subjectOfferingId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Json(new List<SelectListOption>());

        var assignments = await assignmentService.GetAssignmentsAsync(user.Id);
        var scheduleIds = assignments
            .Where(a => a.SubjectOfferingId == subjectOfferingId && a.ExamScheduleId != null)
            .Select(a => a.ExamScheduleId!.Value)
            .Distinct()
            .ToList();

        if (scheduleIds.Count == 0)
        {
            var so = await context.SubjectOfferings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == subjectOfferingId);
            if (so != null)
            {
                scheduleIds = await context.ExamSchedules
                    .Where(es => es.ProgramId == so.ProgramId && es.SemesterId == so.SemesterId && es.IsActive)
                    .Select(es => es.Id)
                    .ToListAsync();
            }
        }

        var schedules = await context.ExamSchedules
            .AsNoTracking()
            .Where(es => scheduleIds.Contains(es.Id))
            .Select(es => new SelectListOption { Value = es.Id, Text = es.ExamScheduleName ?? "Schedule #" + es.Id })
            .ToListAsync();

        return Json(schedules);
    }
}
