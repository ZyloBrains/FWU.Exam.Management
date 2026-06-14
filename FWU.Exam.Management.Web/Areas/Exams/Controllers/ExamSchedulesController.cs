using System.Text;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin,DepartmentAdmin")]
public class ExamSchedulesController(
    IExamScheduleService examScheduleService,
    UserManager<AppUser> userManager,
    AppDbContext context) : Controller
{
    private async Task<(int? collegeId, int? facultyId)> GetScopeAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return (null, null);

        if (User.IsInRole(Role.CollegeAdmin))
            return (user.CollegeId, null);

        if (User.IsInRole(Role.FacultyAdmin))
            return (null, user.FacultyId);

        return (null, null);
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        await examScheduleService.DeactivateExpiredSchedulesAsync();

        var (collegeId, facultyId) = await GetScopeAsync();
        var (items, totalCount) = await examScheduleService.GetExamSchedulesAsync(page, pageSize, search, sort, sortDir, collegeId, facultyId);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    public async Task<IActionResult> ExportToCsv(string search = null)
    {
        var (collegeId, facultyId) = await GetScopeAsync();
        var items = await examScheduleService.GetFilteredItemsAsync(search, collegeId, facultyId);

        var sb = new StringBuilder();
        sb.AppendLine("ID,Exam Schedule Name,Code,Academic Year,Level,Exam Type,Start Date (BS),End Date (BS),Start Date (AD),End Date (AD),Published Date,Start Time,End Time,Is Active,Extended Date,Extended Date Charge,College Approval Date,Admission Card Release Date,Remarks");

        foreach (var item in items)
        {
            sb.AppendLine($"{EscapeCsv(item.Id.ToString())}," +
                          $"{EscapeCsv(item.ExamScheduleName ?? string.Empty)}," +
                          $"{EscapeCsv(item.ExamScheduleCode ?? string.Empty)}," +
                          $"{EscapeCsv(item.AcademicYear?.AcademicYearName ?? string.Empty)}," +
                          $"{EscapeCsv(item.ExamType?.Name ?? string.Empty)}," +
                          $"{EscapeCsv(item.StartDateBs ?? string.Empty)}," +
                          $"{EscapeCsv(item.EndDateBs ?? string.Empty)}," +
                          $"{EscapeCsv(item.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{EscapeCsv(item.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{EscapeCsv(item.PublishedDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{EscapeCsv(item.StartTime.ToString())}," +
                          $"{EscapeCsv(item.EndTime.ToString())}," +
                          $"{(item.IsActive ? "Yes" : "No")}," +
                          $"{EscapeCsv(item.ExtendedDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{item.ExtendedDateCharge}," +
                          $"{EscapeCsv(item.CollegeApprovalDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{EscapeCsv(item.AdmissionCardReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{EscapeCsv(item.Remarks ?? string.Empty)}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "ExamSchedules.csv");
    }

    public async Task<IActionResult> ExportToPdf(string search = null)
    {
        var (collegeId, facultyId) = await GetScopeAsync();
        var items = await examScheduleService.GetFilteredItemsAsync(search, collegeId, facultyId);
        return View("PrintPdf", items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var examSchedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        return View(examSchedule);
    }

    public IActionResult Create()
    {
        var selectLists = examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,AcademicYearId,ProgramId,SemesterId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,StartDate,EndDate,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExtendedDate,ExtendedDateCharge,ExamFee,PracticalSubjectFee,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
    {
        if (ModelState.IsValid)
        {
            await examScheduleService.CreateExamScheduleAsync(examSchedule);
            return RedirectToAction(nameof(Index));
        }
        var selectLists = examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var examSchedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        var selectLists = examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,AcademicYearId,ProgramId,SemesterId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,StartDate,EndDate,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExtendedDate,ExtendedDateCharge,ExamFee,PracticalSubjectFee,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
    {
        if (id != examSchedule.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await examScheduleService.UpdateExamScheduleAsync(examSchedule);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await examScheduleService.ExamScheduleExistsAsync(examSchedule.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        var selectLists = examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var examSchedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        return View(examSchedule);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await examScheduleService.DeleteExamScheduleAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private void PopulateDropdowns(ExamScheduleSelectListsDto selectLists, ExamSchedule? examSchedule = null)
    {
        ViewData["AcademicYearId"] = new SelectList(selectLists.AcademicYears, "Id", "Name", examSchedule?.AcademicYearId);
        ViewData["ExamTypeId"] = new SelectList(selectLists.ExamTypes, "Id", "Name", examSchedule?.ExamTypeId);
        ViewData["ProgramId"] = new SelectList(selectLists.Programs, "Id", "Name", examSchedule?.ProgramId);
        ViewData["SemesterId"] = new SelectList(selectLists.Semesters, "Id", "Name", examSchedule?.SemesterId);
    }
}
