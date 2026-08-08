using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Extensions;
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
[RequirePermission("examschedules.view")]
public class ExamSchedulesController(
    IExamScheduleService examScheduleService,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "StartDate", string sortDir = "desc", int pageSize = 10)
    {
        await examScheduleService.DeactivateExpiredSchedulesAsync();

        var (items, totalCount) = await examScheduleService.GetExamSchedulesAsync(page, pageSize, search, sort, sortDir);

        var scheduleIds = items.Select(i => i.Id).ToList();
        var registrationCounts = await context.ExamRegistrations
            .Where(r => scheduleIds.Contains(r.ExamScheduleId))
            .GroupBy(r => r.ExamScheduleId)
            .Select(g => new { ScheduleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ScheduleId, x => x.Count);

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
        sb.AppendLine("ID,Exam Schedule Name,Code,Academic Year,Level,Exam Type,Start Date (BS),End Date (BS),Start Date (AD),End Date (AD),Published Date,Start Time,End Time,Is Active,Extended Date,Extended Date Charge,College Approval Date,Admission Card Release Date,Remarks");

        foreach (var item in items)
        {
            sb.AppendLine($"{item.Id.ToString().EscapeCsv()}," +
                          $"{(item.ExamScheduleName ?? string.Empty).EscapeCsv()}," +
                          $"{(item.ExamScheduleCode ?? string.Empty).EscapeCsv()}," +
                          $"{(item.AcademicYear?.AcademicYearName ?? string.Empty).EscapeCsv()}," +
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
                          $"{(item.CollegeApprovalDate?.ToString("yyyy-MM-dd") ?? string.Empty).EscapeCsv()}," +
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

        var headers = new[] { "ID", "Exam Schedule Name", "Code", "Academic Year", "Exam Type", "Start Date (BS)", "End Date (BS)", "Start Date (AD)", "End Date (AD)", "Published Date", "Start Time", "End Time", "Is Active", "Extended Date", "Extended Date Charge", "College Approval Date", "Admission Card Release Date", "Remarks" };
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
            worksheet.Cell(row, 4).Value = item.AcademicYear?.AcademicYearName ?? string.Empty;
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
            worksheet.Cell(row, 16).Value = item.CollegeApprovalDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 17).Value = item.AdmissionCardReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 18).Value = item.Remarks ?? string.Empty;
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

        var examSchedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        var registrations = await context.ExamRegistrations
            .Where(r => r.ExamScheduleId == id.Value)
            .ToListAsync();

        var examSlots = await context.ExamSlots
            .Where(es => es.ExamScheduleId == id.Value)
            .Include(es => es.SubjectOffering)
                .ThenInclude(so => so!.SubjectCatalog)
            .Include(es => es.ExamCenter)
            .ToListAsync();

        ViewBag.TotalRegistrations = registrations.Count;
        ViewBag.PaidCount = registrations.Count(r => r.FeeEnclosed.HasValue && r.FeeEnclosed > 0);
        ViewBag.PendingCount = registrations.Count(r => !r.FeeEnclosed.HasValue || r.FeeEnclosed == 0);
        ViewBag.RegisteredCount = registrations.Count(r => r.Status == RegistrationStatus.Registered);
        ViewBag.PendingVerificationCount = registrations.Count(r => r.Status == RegistrationStatus.Pending);
        ViewBag.ExamSlots = examSlots;

        var offeringQuery = context.SubjectOfferings
            .Where(so => so.ProgramId == examSchedule.ProgramId && so.SemesterId == examSchedule.SemesterId);
        var subjectOfferings = await offeringQuery
            .Include(so => so.SubjectCatalog)
            .OrderBy(so => so.DisplayOrder)
            .ThenBy(so => so.Id)
            .ToListAsync();
        ViewBag.SubjectOfferings = subjectOfferings;
        ViewBag.ExistingSlotsByOfferingId = examSlots.ToDictionary(es => es.SubjectOfferingId);

        ViewBag.ExamCenters = await context.ExamCenters
            .Where(ec => ec.IsActive && ec.ExamScheduleId == id.Value)
            .OrderBy(ec => ec.Code)
            .Select(ec => new SelectListItem { Value = ec.Id.ToString(), Text = ec.Code ?? $"Center {ec.Id}" })
            .ToListAsync();

        var batches = await context.Batches
            .Where(b => b.AcademicYearId == examSchedule.AcademicYearId && b.IsActive)
            .OrderBy(b => b.BatchName)
            .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.BatchName })
            .ToListAsync();
        ViewBag.Batches = batches;

        return View(examSchedule);
    }

    [HttpPost]
    [RequirePermission("examschedules.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveExamSlots(int examScheduleId, int batchId, int[] subjectOfferingId, int[] examCenterId, string[]? examDate, string[]? startTime, string[]? endTime, string[]? remarks)
    {
        var schedule = await context.ExamSchedules.FindAsync(examScheduleId);
        if (schedule == null) return NotFound();

        var validOfferingQuery = context.SubjectOfferings
            .Where(so => so.ProgramId == schedule.ProgramId && so.SemesterId == schedule.SemesterId);
        var validOfferingIds = await validOfferingQuery.Select(so => so.Id).ToHashSetAsync();

        var existingSlots = await context.ExamSlots
            .Where(es => es.ExamScheduleId == examScheduleId)
            .ToDictionaryAsync(es => es.SubjectOfferingId);

        var added = 0;
        var updated = 0;

        for (var i = 0; i < subjectOfferingId.Length; i++)
        {
            var offeringId = subjectOfferingId[i];
            if (!validOfferingIds.Contains(offeringId)) continue;

            var centerId = i < examCenterId.Length ? examCenterId[i] : 0;
            var date = (i < (examDate?.Length ?? 0) ? examDate![i] : null)?.Trim();
            var startText = i < (startTime?.Length ?? 0) ? startTime![i] : null;
            var endText = i < (endTime?.Length ?? 0) ? endTime![i] : null;
            var remark = (i < (remarks?.Length ?? 0) ? remarks![i] : null)?.Trim();

            var start = TimeOnly.TryParse(startText, out var startParsed) ? startParsed : schedule.StartTime;
            var end = TimeOnly.TryParse(endText, out var endParsed) ? endParsed : schedule.EndTime;

            if (existingSlots.TryGetValue(offeringId, out var slot))
            {
                if (centerId > 0) slot.ExamCenterId = centerId;
                if (batchId > 0) slot.BatchId = batchId;
                slot.ExamDate = string.IsNullOrEmpty(date) ? null : date;
                slot.StartTime = start;
                slot.EndTime = end;
                slot.Remarks = string.IsNullOrEmpty(remark) ? null : remark;
                updated++;
            }
            else if (centerId > 0 && batchId > 0)
            {
                context.ExamSlots.Add(new ExamSlot
                {
                    ExamScheduleId = examScheduleId,
                    SubjectOfferingId = offeringId,
                    ExamCenterId = centerId,
                    BatchId = batchId,
                    ExamDate = string.IsNullOrEmpty(date) ? null : date,
                    StartTime = start,
                    EndTime = end,
                    Remarks = string.IsNullOrEmpty(remark) ? null : remark,
                    TenantId = schedule.TenantId
                });
                added++;
            }
        }

        if (added > 0 || updated > 0)
            await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Exam subjects saved: {added} added, {updated} updated.";
        return RedirectToAction(nameof(Details), new { id = examScheduleId });
    }

    [HttpPost]
    [RequirePermission("examschedules.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteExamSlot(int id, int examScheduleId)
    {
        var slot = await context.ExamSlots.FindAsync(id);
        if (slot != null)
        {
            context.ExamSlots.Remove(slot);
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Subject removed from exam schedule.";
        }
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
    public async Task<IActionResult> Create([Bind("Id,AcademicYearId,ProgramId,SemesterId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,StartDate,EndDate,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExtendedDate,ExtendedDateCharge,ExamFee,PracticalSubjectFee,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
    {
        if (ModelState.IsValid)
        {
            await examScheduleService.CreateExamScheduleAsync(examSchedule);
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
    public async Task<IActionResult> Edit(int id, [Bind("Id,AcademicYearId,ProgramId,SemesterId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,StartDate,EndDate,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExtendedDate,ExtendedDateCharge,ExamFee,PracticalSubjectFee,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
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
        ViewData["AcademicYearId"] = new SelectList(selectLists.AcademicYears, "Id", "Name", examSchedule?.AcademicYearId);
        ViewData["ExamTypeId"] = new SelectList(selectLists.ExamTypes, "Id", "Name", examSchedule?.ExamTypeId);
        ViewData["ProgramId"] = new SelectList(selectLists.Programs, "Id", "Name", examSchedule?.ProgramId);
        ViewData["SemesterId"] = new SelectList(selectLists.Semesters, "Id", "Name", examSchedule?.SemesterId);
    }

    [HttpGet]
    public async Task<JsonResult> GetSemestersByAcademicYear(int academicYearId, int? programId = null)
    {
        var semesters = await examScheduleService.GetSemestersByAcademicYearAsync(academicYearId, programId);
        return Json(semesters.Select(s => new { id = s.Id, name = s.Name }));
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
            return Json(new { success = true, message = "Exam schedule deleted successfully!" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (DbUpdateException)
        {
            return Json(new { success = false, message = "Cannot delete this record because it is referenced by other records. Please remove or reassign dependent records first." });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "An error occurred while deleting the exam schedule." });
        }
    }

}
