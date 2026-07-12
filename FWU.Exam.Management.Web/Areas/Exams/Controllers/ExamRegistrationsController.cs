using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("examregistration.view")]
public class ExamRegistrationsController(
    IExamRegistrationService examRegistrationService,
    IUserContext userContext,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10, int? examScheduleId = null)
    {
        var (items, totalCount) = await examRegistrationService.GetExamRegistrationsAsync(page, pageSize, search, sort, sortDir, examScheduleId);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;
        ViewBag.ExamScheduleId = examScheduleId;

        ViewData["ExamScheduleId"] = new SelectList(context.ExamSchedules.AsNoTracking().Select(es => new { es.Id, es.ExamScheduleName }), "Id", "ExamScheduleName", examScheduleId);

        return View(items);
    }

    [RequirePermission("examregistration.create")]
    public async Task<IActionResult> Create()
    {
        var selectLists = examRegistrationService.GetSelectListData();
        PopulateDropdowns(selectLists);
        return View();
    }

    [HttpPost]
    [RequirePermission("examregistration.create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ExamScheduleId,CollegeId,AcademicYearId,ExamCenterId,ProgramsId,ExamRollNumber,FeeEnclosed,AttendancePercentage,RegistrationDate,Status,Remarks,IsActive")] ExamRegistration examRegistration)
    {
        if (ModelState.IsValid)
        {
            await examRegistrationService.CreateExamRegistrationAsync(examRegistration);
            return RedirectToAction(nameof(Index));
        }
        var selectLists = examRegistrationService.GetSelectListData(examRegistration);
        PopulateDropdowns(selectLists, examRegistration);
        return View(examRegistration);
    }

    [RequirePermission("examregistration.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var examRegistration = await examRegistrationService.GetExamRegistrationByIdAsync(id.Value);
        if (examRegistration == null) return NotFound();

        var selectLists = examRegistrationService.GetSelectListData(examRegistration);
        PopulateDropdowns(selectLists, examRegistration);
        return View(examRegistration);
    }

    [HttpPost]
    [RequirePermission("examregistration.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ExamScheduleId,CollegeId,AcademicYearId,ExamCenterId,ProgramsId,ExamRollNumber,FeeEnclosed,AttendancePercentage,RegistrationDate,Status,Remarks,IsActive,Sgpa")] ExamRegistration examRegistration)
    {
        if (id != examRegistration.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await examRegistrationService.UpdateExamRegistrationAsync(examRegistration);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await examRegistrationService.ExamRegistrationExistsAsync(examRegistration.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        var selectLists = examRegistrationService.GetSelectListData(examRegistration);
        PopulateDropdowns(selectLists, examRegistration);
        return View(examRegistration);
    }

    [RequirePermission("examregistration.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var examRegistration = await examRegistrationService.GetExamRegistrationByIdAsync(id.Value);
        if (examRegistration == null) return NotFound();

        return View(examRegistration);
    }

    [HttpPost, ActionName("Delete")]
    [RequirePermission("examregistration.delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await examRegistrationService.DeleteExamRegistrationAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("examregistration.verify")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(int id)
    {
        await examRegistrationService.VerifyExamRegistrationAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("examregistration.approve")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        await examRegistrationService.ApproveExamRegistrationAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var examRegistration = await examRegistrationService.GetExamRegistrationByIdAsync(id.Value);
        if (examRegistration == null) return NotFound();

        return View(examRegistration);
    }

    public async Task<IActionResult> ExportToCsv(string? search = null)
    {
        var items = await examRegistrationService.GetFilteredItemsAsync(search);

        var sb = new StringBuilder();
        sb.AppendLine("ID,Exam Schedule,College,Roll Number,Status,Registration Date,Fee,Is Active");

        foreach (var item in items)
        {
            sb.AppendLine($"{item.Id},{EscapeCsv(item.ExamSchedule?.ExamScheduleName ?? "")},{EscapeCsv(item.College?.Name ?? "")},{EscapeCsv(item.ExamRollNumber ?? "")},{item.Status},{item.RegistrationDate?.ToString("yyyy-MM-dd")},{item.FeeEnclosed},{(item.IsActive ? "Yes" : "No")}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "ExamRegistrations.csv");
    }

    public async Task<IActionResult> ExportToPdf(string? search = null)
    {
        var items = await examRegistrationService.GetFilteredItemsAsync(search);
        return View("PrintPdf", items);
    }

    [RequirePermission("examregistration.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            await examRegistrationService.DeleteExamRegistrationAsync(id);
            return Json(new { success = true, message = "Exam registration deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private void PopulateDropdowns(ExamRegistrationSelectListsDto selectLists, ExamRegistration? examRegistration = null)
    {
        ViewData["ExamScheduleId"] = new SelectList(selectLists.ExamSchedules, "Id", "Name", examRegistration?.ExamScheduleId);
        ViewData["CollegeId"] = new SelectList(selectLists.Colleges, "Id", "Name", examRegistration?.CollegeId);
        ViewData["AcademicYearId"] = new SelectList(selectLists.AcademicYears, "Id", "Name", examRegistration?.AcademicYearId);
        ViewData["ProgramsId"] = new SelectList(selectLists.Programs, "Id", "ProgramName", examRegistration?.ProgramsId);
        ViewData["ExamCenterId"] = new SelectList(selectLists.ExamCenters, "Id", "Name", examRegistration?.ExamCenterId);
    }

    private static string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
