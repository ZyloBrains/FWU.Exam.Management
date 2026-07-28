using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("semesters.view")]
public class SemestersController(ISemesterService semesterService, IAcademicYearService academicYearService, IFacultyService facultyService, IUserContext userContext) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Name", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await semesterService.GetSemestersAsync(page, pageSize, search, sort, sortDir, userContext);

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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var items = await semesterService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir, userContext);

        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Number,Year,Start Date,End Date,Remark");

        foreach (var s in items)
        {
            sb.AppendLine($"{EscapeCsv(s.Code)}," +
                           $"{EscapeCsv(s.Name)}," +
                           $"{s.Number}," +
                           $"{s.Year}," +
                           $"{s.StartDate:yyyy-MM-dd}," +
                           $"{s.EndDate:yyyy-MM-dd}," +
                           $"{EscapeCsv(s.Remark)}");
        }

        var fileName = $"Semesters_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var (items, totalCount) = await semesterService.GetSemestersAsync(page, pageSize, search, sort, sortDir, userContext);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var items = await semesterService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir, userContext);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Semesters");

        var headers = new[] { "Code", "Name", "Number", "Year", "Start Date", "End Date", "Remark" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var s in items)
        {
            worksheet.Cell(row, 1).Value = s.Code;
            worksheet.Cell(row, 2).Value = s.Name;
            worksheet.Cell(row, 3).Value = s.Number;
            worksheet.Cell(row, 4).Value = s.Year;
            worksheet.Cell(row, 5).Value = s.StartDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 6).Value = s.EndDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 7).Value = s.Remark;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Semesters_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var semester = await semesterService.GetSemesterByIdAsync(id.Value);
        if (semester == null) return NotFound();

        return View(semester);
    }

    [RequirePermission("semesters.create")]
    public async Task<IActionResult> Create()
    {
        ViewData["AcademicYearId"] = new SelectList(await GetAcademicYearsAsync(), "Id", "AcademicYearName");
        ViewData["FacultyId"] = new SelectList(await GetFacultiesAsync(), "Id", "Name");
        return View();
    }

    [RequirePermission("semesters.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Code,Name,Number,Year,StartDate,EndDate,Remark,AcademicYearId,FacultyId")] Semester semester)
    {
        if (ModelState.IsValid)
        {
            await semesterService.CreateSemesterAsync(semester);
            TempData["SuccessMessage"] = "Semester created successfully!";
            return RedirectToAction(nameof(Index));
        }
        ViewData["AcademicYearId"] = new SelectList(await GetAcademicYearsAsync(), "Id", "AcademicYearName", semester.AcademicYearId);
        ViewData["FacultyId"] = new SelectList(await GetFacultiesAsync(), "Id", "Name", semester.FacultyId);
        return View(semester);
    }

    [RequirePermission("semesters.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var semester = await semesterService.GetSemesterByIdAsync(id.Value);
        if (semester == null) return NotFound();

        ViewData["AcademicYearId"] = new SelectList(await GetAcademicYearsAsync(), "Id", "AcademicYearName", semester.AcademicYearId);
        ViewData["FacultyId"] = new SelectList(await GetFacultiesAsync(), "Id", "Name", semester.FacultyId);
        return View(semester);
    }

    [RequirePermission("semesters.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,Number,Year,StartDate,EndDate,Remark,AcademicYearId,FacultyId")] Semester semester)
    {
        if (id != semester.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await semesterService.UpdateSemesterAsync(semester);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await semesterService.SemesterExistsAsync(semester.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Semester updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        ViewData["AcademicYearId"] = new SelectList(await GetAcademicYearsAsync(), "Id", "AcademicYearName", semester.AcademicYearId);
        ViewData["FacultyId"] = new SelectList(await GetFacultiesAsync(), "Id", "Name", semester.FacultyId);
        return View(semester);
    }

    [RequirePermission("semesters.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var semester = await semesterService.GetSemesterByIdAsync(id.Value);
        if (semester == null) return NotFound();

        return View(semester);
    }

    [RequirePermission("semesters.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await semesterService.DeleteSemesterAsync(id);
            TempData["SuccessMessage"] = "Semester deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
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

    private async Task<List<Domain.Entities.AcademicYear>> GetAcademicYearsAsync()
    {
        var (items, _) = await academicYearService.GetAllAcademicYearsAsync(1, int.MaxValue, null);
        return items;
    }

    private async Task<List<Domain.Entities.Faculty>> GetFacultiesAsync()
    {
        return await facultyService.GetAllFacultiesAsync();
    }
        [RequirePermission("semesters.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await semesterService.DeleteSemesterAsync(id); return Json(new { success = true, message = "Semester deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
