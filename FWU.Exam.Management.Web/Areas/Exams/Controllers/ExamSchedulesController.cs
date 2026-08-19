using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Extensions;
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
[RequirePermission("examschedules.view")]
public class ExamSchedulesController(
    IExamScheduleService examScheduleService,
    IAuditLogWriter auditLogWriter) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "StartDate", string sortDir = "desc", int pageSize = 10)
    {
        var (items, totalCount) = await examScheduleService.GetExamSchedulesAsync(page, pageSize, search, sort, sortDir);

        var scheduleIds = items.Select(i => i.Id).ToList();
        var registrationCounts = await examScheduleService.GetRegistrationCountsAsync(scheduleIds);

        ViewBag.RegistrationCounts = registrationCounts;
        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }


    public async Task<IActionResult> ExportToCsv(string? search = null)
    {
        var items = await examScheduleService.GetFilteredItemsAsync(search);

        var sb = new StringBuilder();
        sb.AppendLine("ID,Exam Schedule Name,Code,Academic Year,Level,Exam Type,Start Date (BS),End Date (BS),Start Date (AD),End Date (AD),Published Date,Start Time,End Time,Is Active,Extended Date,Extended Date Charge,Admission Card Release Date,Remarks");

        foreach (var item in items)
        {
            sb.AppendLine($"{item.Id.ToString().EscapeCsv()}," +
                          $"{(item.ExamScheduleName ?? string.Empty).EscapeCsv()}," +
                          $"{(item.ExamScheduleCode ?? string.Empty).EscapeCsv()}," +
                          $"{(item.SemesterInstance?.AcademicYear?.AcademicYearName ?? string.Empty).EscapeCsv()}," +
                          $"{(item.ExamType?.Name ?? string.Empty).EscapeCsv()}," +
                          $"{(item.StartDateBs ?? string.Empty).EscapeCsv()}," +
                          $"{(item.EndDateBs ?? string.Empty).EscapeCsv()}," +
                          $"{(item.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty).EscapeCsv()}," +
                          $"{(item.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty).EscapeCsv()}," +
                          $"{(item.PublishedDate?.ToString("yyyy-MM-dd") ?? string.Empty).EscapeCsv()}," +
                          $"{item.StartTime.ToString().EscapeCsv()}," +
                          $"{item.EndTime.ToString().EscapeCsv()}," +
                          $"{(item.IsActive ? "Yes" : "No")}," +
                          $"{(item.ExtendedDate?.ToString("yyyy-MM-dd") ?? string.Empty).EscapeCsv()}," +
                          $"{item.ExtendedDateCharge}," +
                          $"{(item.AdmissionCardReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty).EscapeCsv()}," +
                          $"{(item.Remarks ?? string.Empty).EscapeCsv()}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "ExamSchedules.csv");
    }

    public async Task<IActionResult> ExportToPdf(string? search = null)
    {
        var items = await examScheduleService.GetFilteredItemsAsync(search);
        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(string? search = null)
    {
        var items = await examScheduleService.GetFilteredItemsAsync(search);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ExamSchedules");

        var headers = new[] { "ID", "Exam Schedule Name", "Code", "Academic Year", "Exam Type", "Start Date (BS)", "End Date (BS)", "Start Date (AD)", "End Date (AD)", "Published Date", "Start Time", "End Time", "Is Active", "Extended Date", "Extended Date Charge", "Admission Card Release Date", "Remarks" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.Id;
            worksheet.Cell(row, 2).Value = item.ExamScheduleName ?? string.Empty;
            worksheet.Cell(row, 3).Value = item.ExamScheduleCode ?? string.Empty;
            worksheet.Cell(row, 4).Value = item.SemesterInstance?.AcademicYear?.AcademicYearName ?? string.Empty;
            worksheet.Cell(row, 5).Value = item.ExamType?.Name ?? string.Empty;
            worksheet.Cell(row, 6).Value = item.StartDateBs ?? string.Empty;
            worksheet.Cell(row, 7).Value = item.EndDateBs ?? string.Empty;
            worksheet.Cell(row, 8).Value = item.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 9).Value = item.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 10).Value = item.PublishedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 11).Value = item.StartTime.ToString();
            worksheet.Cell(row, 12).Value = item.EndTime.ToString();
            worksheet.Cell(row, 13).Value = item.IsActive ? "Yes" : "No";
            worksheet.Cell(row, 14).Value = item.ExtendedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 15).Value = item.ExtendedDateCharge?.ToString() ?? string.Empty;
            worksheet.Cell(row, 16).Value = item.AdmissionCardReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 17).Value = item.Remarks ?? string.Empty;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExamSchedules.xlsx");
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var details = await examScheduleService.GetExamScheduleDetailsAsync(id.Value);
        if (details == null) return NotFound();

        ViewBag.TotalRegistrations = details.TotalRegistrations;
        ViewBag.PaidCount = details.PaidCount;
        ViewBag.PendingCount = details.PendingCount;
        ViewBag.RegisteredCount = details.RegisteredCount;
        ViewBag.PendingVerificationCount = details.PendingVerificationCount;
        ViewBag.ExamSlots = details.ExamSlots;
        ViewBag.SubjectOfferings = details.SubjectOfferings;
        ViewBag.ExistingSlotsByOfferingId = details.ExistingSlotsByOfferingId;
        ViewBag.ExamCenters = new SelectList(details.ExamCenters.Select(ec => new SelectListItem { Value = ec.Id.ToString(), Text = ec.Name }), "Value", "Text");
        ViewBag.Batches = new SelectList(details.Batches.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name }), "Value", "Text");

        return View(details.Schedule);
    }

    [HttpPost]
    [RequirePermission("examschedules.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveExamSlots(int examScheduleId, int batchId, int[] subjectOfferingId, int[] examCenterId, string[]? examDate, string[]? startTime, string[]? endTime, string[]? remarks)
    {
        var result = await examScheduleService.SaveExamSlotsAsync(examScheduleId, batchId, subjectOfferingId, examCenterId, examDate, startTime, endTime, remarks);

        if (result.Errors.Count > 0)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors.Take(3)) + (result.Errors.Count > 3 ? " (and more)" : "");
            return RedirectToAction(nameof(Details), new { id = examScheduleId });
        }

        await auditLogWriter.LogAsync(ActivityTypes.ExamScheduleUpdated, $"Exam slots saved for schedule {examScheduleId}", new { scheduleId = examScheduleId, added = result.Added, updated = result.Updated }, entityName: "ExamSchedule", entityId: examScheduleId.ToString());
        TempData["SuccessMessage"] = $"Exam subjects saved: {result.Added} added, {result.Updated} updated.";
        return RedirectToAction(nameof(Details), new { id = examScheduleId });
    }

    [HttpPost]
    [RequirePermission("examschedules.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExamSlot(int id, int examScheduleId)
    {
        await examScheduleService.DeleteExamSlotAsync(id);
        await auditLogWriter.LogAsync(ActivityTypes.ExamScheduleUpdated, $"Exam slot {id} removed from schedule {examScheduleId}", new { scheduleId = examScheduleId, slotId = id }, entityName: "ExamSlot", entityId: id.ToString());
        TempData["SuccessMessage"] = "Subject removed from exam schedule.";
        return RedirectToAction(nameof(Details), new { id = examScheduleId });
    }

    [RequirePermission("examschedules.create")]
    public async Task<IActionResult> Create()
    {
        var selectLists = await examScheduleService.GetSelectListDataAsync();
        PopulateDropdowns(selectLists);
        return View();
    }

    [HttpPost]
    [RequirePermission("examschedules.create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ProgramId,SemesterInstanceId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,StartDate,EndDate,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExtendedDate,ExtendedDateCharge,ExamFee,PracticalSubjectFee,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
    {
        if (ModelState.IsValid)
        {
            await examScheduleService.CreateExamScheduleAsync(examSchedule);

            await auditLogWriter.LogAsync(ActivityTypes.ExamScheduleCreated, $"Exam schedule created (Code {examSchedule.ExamScheduleCode})", new { scheduleId = examSchedule.Id, code = examSchedule.ExamScheduleCode, programId = examSchedule.ProgramId, semesterInstanceId = examSchedule.SemesterInstanceId, type = examSchedule.ExamType?.Name }, entityName: "ExamSchedule", entityId: examSchedule.Id.ToString());
            TempData["SuccessMessage"] = "Exam schedule created successfully!";
            return RedirectToAction(nameof(Index));
        }
        var selectLists = await examScheduleService.GetSelectListDataAsync(examSchedule);
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    [RequirePermission("examschedules.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var examSchedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        var selectLists = await examScheduleService.GetSelectListDataAsync(examSchedule);
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    [HttpPost]
    [RequirePermission("examschedules.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ProgramId,SemesterInstanceId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,StartDate,EndDate,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExtendedDate,ExtendedDateCharge,ExamFee,PracticalSubjectFee,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
    {
        if (id != examSchedule.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existing = await examScheduleService.GetExamScheduleByIdAsync(id);
                if (existing is null) return NotFound();
                examSchedule.TenantId = existing.TenantId;
                await examScheduleService.UpdateExamScheduleAsync(examSchedule);
                await examScheduleService.DeactivateExpiredSchedulesAsync();
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                var retrySelectLists = await examScheduleService.GetSelectListDataAsync(examSchedule);
                PopulateDropdowns(retrySelectLists, examSchedule);
                return View(examSchedule);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await examScheduleService.ExamScheduleExistsAsync(examSchedule.Id))
                    return NotFound();
                throw;
            }

            await auditLogWriter.LogAsync(ActivityTypes.ExamScheduleUpdated, $"Exam schedule {examSchedule.Id} updated", new { scheduleId = examSchedule.Id, code = examSchedule.ExamScheduleCode, programId = examSchedule.ProgramId, semesterInstanceId = examSchedule.SemesterInstanceId }, entityName: "ExamSchedule", entityId: examSchedule.Id.ToString());
            TempData["SuccessMessage"] = "Exam schedule updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        var selectLists = await examScheduleService.GetSelectListDataAsync(examSchedule);
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    [RequirePermission("examschedules.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var examSchedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        ViewBag.RegistrationCount = await examScheduleService.GetRegistrationCountAsync(id.Value);

        return View(examSchedule);
    }

    [HttpPost, ActionName("Delete")]
    [RequirePermission("examschedules.delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await examScheduleService.DeleteExamScheduleAsync(id);
            await auditLogWriter.LogAsync(ActivityTypes.ExamScheduleDeleted, $"Exam schedule {id} deleted", new { scheduleId = id }, entityName: "ExamSchedule", entityId: id.ToString());
            TempData["SuccessMessage"] = "Exam schedule deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this record because it is referenced by other records. Please remove or reassign dependent records first.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while deleting: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    private void PopulateDropdowns(ExamScheduleSelectListsDto selectLists, ExamSchedule? examSchedule = null)
    {
        int? academicYearId = examSchedule?.SemesterInstanceId > 0
            ? examSchedule.SemesterInstance?.AcademicYearId
            : null;
        ViewData["AcademicYearId"] = new SelectList(selectLists.AcademicYears, "Id", "Name", academicYearId);
        ViewData["ExamTypeId"] = new SelectList(selectLists.ExamTypes, "Id", "Name", examSchedule?.ExamTypeId);
        ViewData["ProgramId"] = new SelectList(selectLists.Programs, "Id", "Name", examSchedule?.ProgramId);
        ViewData["SemesterInstanceId"] = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text", examSchedule?.SemesterInstanceId);
    }

    [HttpGet]
    public async Task<JsonResult> GetSemestersByAcademicYear(int academicYearId, int programId)
    {
        var semesters = await examScheduleService.GetSemestersByAcademicYearAsync(academicYearId, programId);
        return Json(semesters);
    }

    [HttpGet]
    public async Task<JsonResult> GetProgramsByAcademicYear(int academicYearId)
    {
        var programs = await examScheduleService.GetProgramsByAcademicYearAsync(academicYearId);
        return Json(programs);
    }

    [HttpGet]
    public JsonResult ConvertBsToAd(string bsDate)
    {
        var parts = bsDate.Split('-');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var y) || !int.TryParse(parts[1], out var m) || !int.TryParse(parts[2], out var d))
            return Json(new { adDate = (string?)null });
        var ad = NepaliCalendarHelper.BsToAd(y, m, d);
        return Json(new { adDate = ad?.ToString("yyyy-MM-dd") });
    }

    [HttpGet]
    public JsonResult ConvertAdToBs(string adDate)
    {
        if (!DateTime.TryParse(adDate, out var dt))
            return Json(new { bsDate = (string?)null });
        var bs = NepaliCalendarHelper.AdToBs(dt);
        return Json(new { bsDate = $"{bs.Year:D4}-{bs.Month:D2}-{bs.Day:D2}" });
    }

    [RequirePermission("examschedules.delete")]
    [HttpGet]
    public async Task<IActionResult> GetDeletePreview(int id)
    {
        try
        {
            var preview = await examScheduleService.GetDeletePreviewAsync(id);
            return Json(new { success = true, scheduleName = preview.ScheduleName, items = preview.Items.Select(i => new { label = i.Label, count = i.Count }) });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [RequirePermission("examschedules.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            await examScheduleService.DeleteExamScheduleAsync(id);
            await auditLogWriter.LogAsync(ActivityTypes.ExamScheduleDeleted, $"Exam schedule {id} deleted", new { scheduleId = id }, entityName: "ExamSchedule", entityId: id.ToString());
            return Json(new { success = true, message = "Exam schedule deleted successfully!" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (DbUpdateException)
        {
            return Json(new { success = false, message = "Cannot delete this exam schedule because it is referenced by other records." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"An error occurred while deleting: {ex.Message}" });
        }
    }

}
