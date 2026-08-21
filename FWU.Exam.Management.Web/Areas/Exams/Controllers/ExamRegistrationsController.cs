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
[RequirePermission("examregistration.view")]
public class ExamRegistrationsController(
    IExamRegistrationService examRegistrationService,
    IPermissionService permissionService,
    UserManager<AppUser> userManager,
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
        var selectLists = await examRegistrationService.GetSelectListDataAsync();
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
            TempData["SuccessMessage"] = "Exam registration created successfully!";
            return RedirectToAction(nameof(Index));
        }
        var selectLists = await examRegistrationService.GetSelectListDataAsync(examRegistration);
        PopulateDropdowns(selectLists, examRegistration);
        return View(examRegistration);
    }

    [RequirePermission("examregistration.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var examRegistration = await examRegistrationService.GetExamRegistrationByIdAsync(id.Value);
        if (examRegistration == null) return NotFound();

        var selectLists = await examRegistrationService.GetSelectListDataAsync(examRegistration);
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
            TempData["SuccessMessage"] = "Exam registration updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        var selectLists = await examRegistrationService.GetSelectListDataAsync(examRegistration);
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
        try
        {
            await examRegistrationService.DeleteExamRegistrationAsync(id);
            TempData["SuccessMessage"] = "Exam registration deleted successfully!";
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

    [RequirePermission("examregistration.verify")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(int id, int? academicYearId, int? levelId, int? examScheduleId, string? search, int page = 1)
    {
        await examRegistrationService.VerifyExamRegistrationAsync(id);
        TempData["SuccessMessage"] = "Student exam form approved successfully!";
        return RedirectToAction(nameof(StudentForms), new { academicYearId, levelId, examScheduleId, search, page });
    }

    [RequirePermission("examregistration.approve")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, int? academicYearId, int? levelId, int? examScheduleId, string? search, int page = 1)
    {
        await examRegistrationService.ApproveExamRegistrationAsync(id);
        TempData["SuccessMessage"] = "Exam registration approved successfully!";
        return RedirectToAction(nameof(StudentForms), new { academicYearId, levelId, examScheduleId, search, page });
    }

    [RequirePermission("examregistration.view")]
    public async Task<IActionResult> StudentForms(int? academicYearId, int? levelId, int? examScheduleId, string? search, int page = 1, int pageSize = 25)
    {
        var result = await examRegistrationService.GetStudentExamFormsAsync(academicYearId, levelId, examScheduleId, search, page, pageSize);

        ViewBag.TotalCount = result.TotalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.AcademicYearId = academicYearId;
        ViewBag.LevelId = levelId;
        ViewBag.ExamScheduleId = examScheduleId;
        ViewBag.PaymentConfirmedCount = result.PaymentConfirmedCount;
        ViewBag.AdmitCardGeneratedCount = result.AdmitCardGeneratedCount;
        ViewBag.PendingAdmitCardCount = result.PendingAdmitCardCount;
        ViewBag.PendingApprovalCount = result.PendingApprovalCount;
        ViewBag.PendingBySchedule = result.PendingBySchedule;

        ViewBag.AcademicYearOptions = await examRegistrationService.GetFilterAcademicYearsAsync();
        ViewBag.LevelOptions = academicYearId.HasValue
            ? await examRegistrationService.GetFilterLevelsAsync(academicYearId.Value)
            : new List<SelectOption>();
        ViewBag.ExamScheduleOptions = academicYearId.HasValue && levelId.HasValue
            ? await examRegistrationService.GetFilterExamSchedulesAsync(academicYearId.Value, levelId.Value)
            : new List<SelectOption>();

        return View(result.Forms);
    }

    [HttpGet]
    [RequirePermission("examregistration.view")]
    public async Task<IActionResult> StudentFormReview(int id, bool showActions = true, int? academicYearId = null, int? levelId = null, int? examScheduleId = null, string? search = null, int page = 1)
    {
        var form = await examRegistrationService.GetStudentExamFormDetailAsync(id);
        if (form == null) return NotFound();

        var currentUser = await userManager.GetUserAsync(User);
        var userPerms = currentUser != null
            ? await permissionService.GetUserPermissionsAsync(currentUser.Id)
            : new List<string>();

        ViewBag.ShowActions = showActions;
        ViewBag.CanAdminApprove = userPerms.Contains("examregistration.approve");
        ViewBag.CanEditSubjects = userPerms.Contains("examregistration.edit");
        ViewBag.AcademicYearId = academicYearId;
        ViewBag.LevelId = levelId;
        ViewBag.ExamScheduleId = examScheduleId;
        ViewBag.Search = search;
        ViewBag.Page = page;
        return PartialView("_StudentFormReview", form);
    }

    [HttpGet]
    [RequirePermission("examregistration.edit")]
    public async Task<IActionResult> StudentFormEditableSubjects(int id)
    {
        var model = await examRegistrationService.GetEditableSubjectsAsync(id);
        if (model == null) return NotFound();

        return PartialView("_StudentFormSubjectsEdit", model);
    }

    [HttpPost]
    [RequirePermission("examregistration.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStudentFormSubjects(int id, List<int> subjectOfferingIds)
    {
        var (success, message) = await examRegistrationService.UpdateRegistrationSubjectsAsync(id, subjectOfferingIds);
        return Json(new { success, message });
    }

    [HttpGet]
    [RequirePermission("examregistration.view")]
    public async Task<IActionResult> GetFilterAcademicYears()
    {
        return Json(await examRegistrationService.GetFilterAcademicYearsAsync());
    }

    [HttpGet]
    [RequirePermission("examregistration.view")]
    public async Task<IActionResult> GetFilterLevels(int academicYearId)
    {
        return Json(await examRegistrationService.GetFilterLevelsAsync(academicYearId));
    }

    [HttpGet]
    [RequirePermission("examregistration.view")]
    public async Task<IActionResult> GetFilterExamSchedules(int academicYearId, int levelId)
    {
        return Json(await examRegistrationService.GetFilterExamSchedulesAsync(academicYearId, levelId));
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
            sb.AppendLine($"{item.Id},{(item.ExamSchedule?.ExamScheduleName ?? "").EscapeCsv()},{(item.College?.Name ?? "").EscapeCsv()},{(item.ExamRollNumber ?? "").EscapeCsv()},{item.Status},{item.RegistrationDate?.ToString("yyyy-MM-dd")},{item.FeeEnclosed},{(item.IsActive ? "Yes" : "No")}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "ExamRegistrations.csv");
    }

    public async Task<IActionResult> ExportToPdf(string? search = null)
    {
        var items = await examRegistrationService.GetFilteredItemsAsync(search);
        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(string? search = null)
    {
        var items = await examRegistrationService.GetFilteredItemsAsync(search);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ExamRegistrations");

        var headers = new[] { "ID", "Exam Schedule", "College", "Roll Number", "Status", "Registration Date", "Fee", "Is Active" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.Gray;
        }

        int row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.Id;
            worksheet.Cell(row, 2).Value = item.ExamSchedule?.ExamScheduleName ?? "";
            worksheet.Cell(row, 3).Value = item.College?.Name ?? "";
            worksheet.Cell(row, 4).Value = item.ExamRollNumber ?? "";
            worksheet.Cell(row, 5).Value = item.Status.ToString();
            worksheet.Cell(row, 6).Value = item.RegistrationDate?.ToString("yyyy-MM-dd") ?? "";
            worksheet.Cell(row, 7).Value = item.FeeEnclosed;
            worksheet.Cell(row, 8).Value = item.IsActive ? "Yes" : "No";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExamRegistrations.xlsx");
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
        ViewData["ProgramsId"] = new SelectList(selectLists.Programs, "Id", "Name", examRegistration?.ProgramsId);
        ViewData["ExamCenterId"] = new SelectList(selectLists.ExamCenters, "Id", "Name", examRegistration?.ExamCenterId);
    }

}
