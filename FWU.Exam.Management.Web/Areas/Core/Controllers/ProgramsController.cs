using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("programs.view")]
public class ProgramsController(IProgramService programService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "ProgramCode", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await programService.GetProgramsAsync(page, pageSize, search, sort, sortDir);

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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "ProgramCode", string sortDir = "asc")
    {
        var items = await programService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Program Code,Program Name,Short Name,Level,Faculty,Board,Program Period Type,Duration,Grand Total Marks,Has Multiple Intakes,Number of Seats,Scholarship Seats,Roll Number Prefix,Remarks,Status");

        foreach (var p in items)
        {
            sb.AppendLine($"{EscapeCsv(p.ProgramCode)}," +
                           $"{EscapeCsv(p.ProgramName)}," +
                           $"{EscapeCsv(p.ShortName)}," +
                           $"{EscapeCsv(p.Level?.LevelName)}," +
                            $"{EscapeCsv(p.Department?.DepartmentCode)}," +
                           $"{EscapeCsv(p.Board?.BoardName)}," +
                           $"{p.Duration}," +
                           $"{p.GrandTotalMarks}," +
                           $"{(p.HasMultipleIntakes ? "Yes" : "No")}," +
                           $"{p.NumberOfSeats}," +
                           $"{p.ScholarshipSeats}," +
                           $"{EscapeCsv(p.RollNumberPrefix)}," +
                           $"{EscapeCsv(p.Remarks)}," +
                           $"{(p.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Programs_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "ProgramCode", string sortDir = "asc")
    {
        var (items, totalCount) = await programService.GetProgramsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "ProgramCode", string sortDir = "asc")
    {
        var items = await programService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Programs");

        var headers = new[] { "Program Code", "Program Name", "Short Name", "Level", "Faculty", "Board", "Program Period Type", "Duration", "Grand Total Marks", "Has Multiple Intakes", "Number of Seats", "Scholarship Seats", "Roll Number Prefix", "Remarks", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var p in items)
        {
            worksheet.Cell(row, 1).Value = p.ProgramCode;
            worksheet.Cell(row, 2).Value = p.ProgramName;
            worksheet.Cell(row, 3).Value = p.ShortName;
            worksheet.Cell(row, 4).Value = p.Level?.LevelName;
            worksheet.Cell(row, 5).Value = p.Department?.DepartmentCode;
            worksheet.Cell(row, 6).Value = p.Board?.BoardName;
            worksheet.Cell(row, 7).Value = "";
            worksheet.Cell(row, 8).Value = p.Duration;
            worksheet.Cell(row, 9).Value = p.GrandTotalMarks;
            worksheet.Cell(row, 10).Value = p.HasMultipleIntakes ? "Yes" : "No";
            worksheet.Cell(row, 11).Value = p.NumberOfSeats;
            worksheet.Cell(row, 12).Value = p.ScholarshipSeats;
            worksheet.Cell(row, 13).Value = p.RollNumberPrefix;
            worksheet.Cell(row, 14).Value = p.Remarks;
            worksheet.Cell(row, 15).Value = p.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Programs_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var program = await programService.GetProgramByIdAsync(id.Value);
        if (program == null) return NotFound();

        return View(program);
    }

    [RequirePermission("programs.create")]
    public async Task<IActionResult> Create()
    {
        var (boards, departments, levels) = await programService.GetSelectListsAsync();
        ViewData["BoardId"] = new SelectList(boards, "Id", "BoardName");
        ViewData["FacultyId"] = new SelectList(departments, "Id", "DepartmentCode");
        ViewData["LevelId"] = new SelectList(levels, "Id", "LevelName");
        return View();
    }

    [RequirePermission("programs.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,LevelId,DepartmentId,BoardId,ProgramCode,ProgramName,ShortName,Duration,GrandTotalMarks,HasMultipleIntakes,NumberOfSeats,ScholarshipSeats,Remarks,IsActive,RollNumberPrefix")] Program program)
    {
        if (ModelState.IsValid)
        {
            await programService.CreateProgramAsync(program);
            return RedirectToAction(nameof(Index));
        }

        var (boards, departments, levels) = await programService.GetSelectListsAsync(program.BoardId, program.DepartmentId, program.LevelId);
        ViewData["BoardId"] = new SelectList(boards, "Id", "BoardName", program.BoardId);
        ViewData["FacultyId"] = new SelectList(departments, "Id", "DepartmentCode", program.DepartmentId);
        ViewData["LevelId"] = new SelectList(levels, "Id", "LevelName", program.LevelId);
        return View(program);
    }

    [RequirePermission("programs.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var program = await programService.GetProgramByIdAsync(id.Value);
        if (program == null) return NotFound();

        var (boards, departments, levels) = await programService.GetSelectListsAsync(program.BoardId, program.DepartmentId, program.LevelId);
        ViewData["BoardId"] = new SelectList(boards, "Id", "BoardName", program.BoardId);
        ViewData["FacultyId"] = new SelectList(departments, "Id", "DepartmentCode", program.DepartmentId);
        ViewData["LevelId"] = new SelectList(levels, "Id", "LevelName", program.LevelId);
        return View(program);
    }

    [RequirePermission("programs.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LevelId,DepartmentId,BoardId,ProgramCode,ProgramName,ShortName,Duration,GrandTotalMarks,HasMultipleIntakes,NumberOfSeats,ScholarshipSeats,Remarks,IsActive,RollNumberPrefix")] Program program)
    {
        if (id != program.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await programService.UpdateProgramAsync(program);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await programService.ProgramExistsAsync(program.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        var (boards, departments, levels) = await programService.GetSelectListsAsync(program.BoardId, program.DepartmentId, program.LevelId);
        ViewData["BoardId"] = new SelectList(boards, "Id", "BoardName", program.BoardId);
        ViewData["FacultyId"] = new SelectList(departments, "Id", "DepartmentCode", program.DepartmentId);
        ViewData["LevelId"] = new SelectList(levels, "Id", "LevelName", program.LevelId);
        return View(program);
    }

    [RequirePermission("programs.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var program = await programService.GetProgramByIdAsync(id.Value);
        if (program == null) return NotFound();

        return View(program);
    }

    [RequirePermission("programs.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await programService.DeleteProgramAsync(id);
        return RedirectToAction(nameof(Index));
    }
        [RequirePermission("PLACEHOLDER_PERMISSION")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await programService.DeleteProgramAsync(id); return Json(new { success = true, message = "Program deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
