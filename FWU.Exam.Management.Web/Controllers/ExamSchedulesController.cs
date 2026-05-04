using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

public class ExamSchedulesController : Controller
{
    private readonly IExamScheduleService _examScheduleService;

    public ExamSchedulesController(IExamScheduleService examScheduleService)
    {
        _examScheduleService = examScheduleService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _examScheduleService.GetExamSchedulesAsync(page, pageSize, search, sort, sortDir);

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
        var items = await _examScheduleService.GetFilteredItemsAsync(search);

        var sb = new StringBuilder();
        sb.AppendLine("ID,Exam Schedule Name,Code,Academic Year,Level,Exam Type,Start Date (BS),End Date (BS),Published Date,Start Time,End Time,Is Active,Extended Date,Extended Date Charge,College Approval Date,Admission Card Release Date,Remarks");

        foreach (var item in items)
        {
            sb.AppendLine($"{EscapeCsv(item.Id.ToString())}," +
                          $"{EscapeCsv(item.ExamScheduleName ?? string.Empty)}," +
                          $"{EscapeCsv(item.ExamScheduleCode ?? string.Empty)}," +
                          $"{EscapeCsv(item.AcademicYear?.AcademicYearName ?? string.Empty)}," +
                          $"{EscapeCsv(item.Level?.LevelName ?? string.Empty)}," +
                          $"{EscapeCsv(item.ExamType?.Name ?? string.Empty)}," +
                          $"{EscapeCsv(item.StartDateBs ?? string.Empty)}," +
                          $"{EscapeCsv(item.EndDateBs ?? string.Empty)}," +
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
        var items = await _examScheduleService.GetFilteredItemsAsync(search);
        return View("PrintPdf", items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var examSchedule = await _examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        return View(examSchedule);
    }

    public IActionResult Create()
    {
        var selectLists = _examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,AcademicYearId,LevelId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExamScheduleParentId,ExtendedDate,ExtendedDateCharge,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
    {
        if (ModelState.IsValid)
        {
            await _examScheduleService.CreateExamScheduleAsync(examSchedule);
            return RedirectToAction(nameof(Index));
        }
        var selectLists = _examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var examSchedule = await _examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        var selectLists = _examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,AcademicYearId,LevelId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExamScheduleParentId,ExtendedDate,ExtendedDateCharge,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
    {
        if (id != examSchedule.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _examScheduleService.UpdateExamScheduleAsync(examSchedule);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _examScheduleService.ExamScheduleExistsAsync(examSchedule.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        var selectLists = _examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var examSchedule = await _examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        return View(examSchedule);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _examScheduleService.DeleteExamScheduleAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private void PopulateDropdowns(object selectLists, ExamSchedule? examSchedule = null)
    {
        // The selectLists object contains AcademicYears, ExamTypes, Levels
        ViewData["AcademicYearId"] = new SelectList(((dynamic)selectLists).AcademicYears, "Id", "Name", examSchedule?.AcademicYearId);
        ViewData["ExamTypeId"] = new SelectList(((dynamic)selectLists).ExamTypes, "Id", "Name", examSchedule?.ExamTypeId);
        ViewData["LevelId"] = new SelectList(((dynamic)selectLists).Levels, "Id", "Name", examSchedule?.LevelId);
    }
}
