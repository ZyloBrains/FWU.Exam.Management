using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Subjects.Controllers;

[Area("Subjects")]
[RequirePermission("subjects.view")]
public class SubjectCatalogsController : Controller
{
    private readonly ISubjectCatalogService _subjectCatalogService;

    public SubjectCatalogsController(ISubjectCatalogService subjectCatalogService)
    {
        _subjectCatalogService = subjectCatalogService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "SubjectName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _subjectCatalogService.GetSubjectCatalogsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> ImportExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "No file uploaded";
            return RedirectToAction(nameof(Index));
        }

        var fileExtension = Path.GetExtension(file.FileName);
        if (!string.Equals(fileExtension, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Please upload an Excel file in .xlsx format.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                    if (rowCount < 2)
                    {
                        TempData["ErrorMessage"] = "Excel file is empty";
                        return RedirectToAction(nameof(Index));
                    }

                    var validTypeIds = (await _subjectCatalogService.GetSelectListsAsync()).Select(st => st.Id).ToHashSet();

                    if (validTypeIds.Count == 0)
                    {
                        TempData["ErrorMessage"] = "No SubjectTypes found in the system. Seed the database first.";
                        return RedirectToAction(nameof(Index));
                    }

                    var catalogs = new List<SubjectCatalog>();
                    var errors = new List<string>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var subjectCode = worksheet.Cell(row, 1).GetString().Trim();
                        if (string.IsNullOrEmpty(subjectCode))
                        {
                            errors.Add($"Row {row}: SubjectCode is required");
                            continue;
                        }

                        var subjectName = worksheet.Cell(row, 2).GetString().Trim();
                        if (string.IsNullOrEmpty(subjectName))
                        {
                            errors.Add($"Row {row}: SubjectName is required");
                            continue;
                        }

                        var shortName = worksheet.Cell(row, 3).GetString().Trim();
                        var creditHours = int.TryParse(worksheet.Cell(row, 4).GetString().Trim(), out var ch) ? ch : 3;

                        var subjectTypeIdStr = worksheet.Cell(row, 5).GetString().Trim();
                        if (!int.TryParse(subjectTypeIdStr, out var subjectTypeId))
                        {
                            errors.Add($"Row {row}: SubjectTypeID must be a number, got '{subjectTypeIdStr}'");
                            continue;
                        }

                        if (!validTypeIds.Contains(subjectTypeId))
                        {
                            errors.Add($"Row {row}: SubjectTypeID {subjectTypeId} is not valid. Valid IDs: {string.Join(", ", validTypeIds.OrderBy(x => x))}");
                            continue;
                        }

                        var isActiveStr = worksheet.Cell(row, 6).GetString().Trim();

                        catalogs.Add(new SubjectCatalog
                        {
                            SubjectCode = subjectCode,
                            SubjectName = subjectName,
                            ShortName = string.IsNullOrEmpty(shortName) ? null : shortName,
                            CreditHours = creditHours,
                            SubjectTypeId = subjectTypeId,
                            IsActive = isActiveStr.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
                                       isActiveStr.Equals("True", StringComparison.OrdinalIgnoreCase) ||
                                       isActiveStr.Equals("Y", StringComparison.OrdinalIgnoreCase) ||
                                       isActiveStr.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                                       string.IsNullOrEmpty(isActiveStr)
                        });
                    }

                    if (catalogs.Count > 0)
                    {
                        var existingCodes = await _subjectCatalogService.GetExistingSubjectCodesAsync();

                        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var deduplicated = new List<SubjectCatalog>();

                        foreach (var c in catalogs)
                        {
                            if (existingCodes.Contains(c.SubjectCode, StringComparer.OrdinalIgnoreCase))
                            {
                                errors.Add($"SubjectCode '{c.SubjectCode}' already exists in the system");
                                continue;
                            }
                            if (!seenCodes.Add(c.SubjectCode))
                            {
                                errors.Add($"Duplicate SubjectCode '{c.SubjectCode}' in Excel");
                                continue;
                            }
                            deduplicated.Add(c);
                        }

                        if (deduplicated.Count > 0)
                        {
                            try
                            {
                                await _subjectCatalogService.BulkCreateAsync(deduplicated);
                                TempData["SuccessMessage"] = $"Imported {deduplicated.Count} subject(s) successfully.";
                            }
                            catch (Exception saveEx)
                            {
                                errors.Add($"Database error: {saveEx.InnerException?.Message ?? saveEx.Message}");
                            }
                        }
                    }

                    if (errors.Count > 0)
                        TempData["ErrorMessage"] = string.Join(Environment.NewLine, errors.Take(30)) +
                                                   (errors.Count > 30 ? $"\n... and {errors.Count - 30} more error(s)" : "");

                    return RedirectToAction(nameof(Index));
                }
            }
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException?.Message ?? ex.Message;
            TempData["ErrorMessage"] = $"Error processing file: {detail}";
            return RedirectToAction(nameof(Index));
        }
    }

    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "SubjectName", string sortDir = "asc")
    {
        var items = await _subjectCatalogService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Code,Subject Name,Short Name,Credit Hours,Type,Status");

        foreach (var s in items)
        {
            sb.AppendLine($"{EscapeCsv(s.SubjectCode)}," +
                           $"{EscapeCsv(s.SubjectName)}," +
                           $"{EscapeCsv(s.ShortName ?? "-")}," +
                           $"{s.CreditHours}," +
                           $"{EscapeCsv(s.SubjectType?.Name ?? "-")}," +
                           $"{(s.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"SubjectCatalogs_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "SubjectName", string sortDir = "asc")
    {
        var items = await _subjectCatalogService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Subject Catalogs");

        var headers = new[] { "Code", "Subject Name", "Short Name", "Credit Hours", "Type", "Status" };
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
            worksheet.Cell(row, 1).Value = s.SubjectCode ?? "-";
            worksheet.Cell(row, 2).Value = s.SubjectName ?? "-";
            worksheet.Cell(row, 3).Value = s.ShortName ?? "-";
            worksheet.Cell(row, 4).Value = s.CreditHours;
            worksheet.Cell(row, 5).Value = s.SubjectType?.Name ?? "-";
            worksheet.Cell(row, 6).Value = s.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"SubjectCatalogs_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "SubjectName", string sortDir = "asc")
    {
        var (items, totalCount) = await _subjectCatalogService.GetSubjectCatalogsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var subjectCatalog = await _subjectCatalogService.GetSubjectCatalogByIdAsync(id.Value);
        if (subjectCatalog == null) return NotFound();

        return View(subjectCatalog);
    }

    [RequirePermission("subjects.create")]
    public async Task<IActionResult> Create()
    {
        var subjectTypes = await _subjectCatalogService.GetSelectListsAsync();
        ViewData["SubjectTypeId"] = new SelectList(subjectTypes, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("subjects.create")]
    public async Task<IActionResult> Create([Bind("Id,SubjectCode,SubjectName,ShortName,Description,CreditHours,SubjectTypeId,IsActive")] SubjectCatalog subjectCatalog)
    {
        if (ModelState.IsValid)
        {
            await _subjectCatalogService.CreateSubjectCatalogAsync(subjectCatalog);
            TempData["SuccessMessage"] = "Subject catalog created successfully.";
            return RedirectToAction(nameof(Index));
        }
        var subjectTypes = await _subjectCatalogService.GetSelectListsAsync(subjectCatalog.SubjectTypeId);
        ViewData["SubjectTypeId"] = new SelectList(subjectTypes, "Id", "Name", subjectCatalog.SubjectTypeId);
        return View(subjectCatalog);
    }

    [RequirePermission("subjects.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var subjectCatalog = await _subjectCatalogService.GetSubjectCatalogByIdAsync(id.Value);
        if (subjectCatalog == null) return NotFound();

        var subjectTypes = await _subjectCatalogService.GetSelectListsAsync(subjectCatalog.SubjectTypeId);
        ViewData["SubjectTypeId"] = new SelectList(subjectTypes, "Id", "Name", subjectCatalog.SubjectTypeId);
        return View(subjectCatalog);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("subjects.edit")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,SubjectCode,SubjectName,ShortName,Description,CreditHours,SubjectTypeId,IsActive")] SubjectCatalog subjectCatalog)
    {
        if (id != subjectCatalog.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _subjectCatalogService.UpdateSubjectCatalogAsync(subjectCatalog);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _subjectCatalogService.SubjectCatalogExistsAsync(subjectCatalog.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Subject catalog updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        var subjectTypes = await _subjectCatalogService.GetSelectListsAsync(subjectCatalog.SubjectTypeId);
        ViewData["SubjectTypeId"] = new SelectList(subjectTypes, "Id", "Name", subjectCatalog.SubjectTypeId);
        return View(subjectCatalog);
    }

    [RequirePermission("subjects.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var subjectCatalog = await _subjectCatalogService.GetSubjectCatalogByIdAsync(id.Value);
        if (subjectCatalog == null) return NotFound();

        return View(subjectCatalog);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission("subjects.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _subjectCatalogService.DeleteSubjectCatalogAsync(id);
            TempData["SuccessMessage"] = "Subject catalog deleted successfully.";
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
        [RequirePermission("subjects.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await _subjectCatalogService.DeleteSubjectCatalogAsync(id); return Json(new { success = true, message = "Subject deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
