using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("boards.view")]
public class BoardsController(IBoardService boardService, ICountryService countryService) : Controller
{

    // GET: Boards with pagination, search, and sorting
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "BoardName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await boardService.GetBoardsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    // Helper method to escape CSV fields

    // Export to CSV (Current Page with pagination)
    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "BoardName", string sortDir = "asc")
    {
        var (items, totalCount) = await boardService.GetBoardsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();

        // CSV header
        sb.AppendLine("Board Name,Remarks,Status");

        foreach (var b in items)
        {
            sb.AppendLine($"{b.BoardName.EscapeCsv()}," +
                           //$"{(b.Country?.CountryName ?? "").EscapeCsv()}," +
                           $"{b.Remarks.EscapeCsv()}," +
                           $"{(b.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Boards_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    // Export to PDF (Current Page with pagination)
    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "BoardName", string sortDir = "asc")
    {
        var (items, totalCount) = await boardService.GetBoardsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "BoardName", string sortDir = "asc")
    {
        var (items, totalCount) = await boardService.GetBoardsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Boards");

        var headers = new[] { "Board Name", "Remarks", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var b in items)
        {
            worksheet.Cell(row, 1).Value = b.BoardName;
            worksheet.Cell(row, 2).Value = b.Remarks;
            worksheet.Cell(row, 3).Value = b.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Boards_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // GET: Boards/Create
    [RequirePermission("boards.create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.CountryList = new SelectList(await countryService.GetAllAsync(), "Id", "CountryName");
        return View();
    }

    // POST: Boards/Create
    [RequirePermission("boards.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,CountryId,BoardName,Remarks,IsActive")] Board board)
    {
        if (ModelState.IsValid)
        {
            await boardService.CreateBoardAsync(board);
            TempData["SuccessMessage"] = "Board created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(board);
    }

    // GET: Boards/Edit/5
    [RequirePermission("boards.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var board = await boardService.GetBoardByIdAsync(id.Value);
        if (board == null)
        {
            return NotFound();
        }
        ViewBag.CountryList = new SelectList(await countryService.GetAllAsync(), "Id", "CountryName", board.CountryId);
        return View(board);
    }

    // POST: Boards/Edit/5
    [RequirePermission("boards.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CountryId,BoardName,Remarks,IsActive")] Board board)
    {
        if (id != board.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await boardService.UpdateBoardAsync(board);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await boardService.BoardExistsAsync(board.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["SuccessMessage"] = "Board updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(board);
    }

    // GET: Boards/Delete/5
    [RequirePermission("boards.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var board = await boardService.GetBoardByIdAsync(id.Value);
        if (board == null)
        {
            return NotFound();
        }

        return View(board);
    }

    // POST: Boards/Delete/5
    [RequirePermission("boards.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await boardService.DeleteBoardAsync(id);
            TempData["SuccessMessage"] = "Board deleted successfully!";
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
        [RequirePermission("boards.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await boardService.DeleteBoardAsync(id); return Json(new { success = true, message = "Board deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> SearchCountries(string q)
    {
        var countries = await countryService.GetAllAsync();
        if (!string.IsNullOrEmpty(q))
            countries = countries.Where(c => c.CountryName!.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        return Json(countries.Select(c => new { id = c.Id, text = c.CountryName }));
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CreateCountry([FromBody] CountryCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { success = false, message = "Country name is required." });

        var existing = await countryService.FindByNameAsync(request.Name.Trim());
        if (existing != null)
            return Ok(new { id = existing.Id, text = existing.CountryName });

        var country = await countryService.CreateAsync(request.Name.Trim());
        return Ok(new { id = country.Id, text = country.CountryName });
    }

    public class CountryCreateRequest
    {
        public string? Name { get; set; }
    }

}
