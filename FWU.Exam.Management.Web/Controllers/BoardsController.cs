using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FWU.Exam.Management.Web.Controllers;

public class BoardsController : Controller
{
    private readonly IBoardService _boardService;

    public BoardsController(IBoardService boardService)
    {
        _boardService = boardService;
    }

    // GET: Boards with pagination, search, and sorting
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "BoardName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _boardService.GetBoardsAsync(page, pageSize, search, sort, sortDir);

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
    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    // Export to CSV (Current Page with pagination)
    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "BoardName", string sortDir = "asc")
    {
        var (items, totalCount) = await _boardService.GetBoardsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();

        // CSV header
        sb.AppendLine("Board Name,Country,Remarks,Status");

        foreach (var b in items)
        {
            sb.AppendLine($"{EscapeCsv(b.BoardName)}," +
                           //$"{EscapeCsv(b.Country?.CountryName ?? "")}," +
                           $"{EscapeCsv(b.Remarks)}," +
                           $"{(b.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Boards_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    // Export to PDF (Current Page with pagination)
    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "BoardName", string sortDir = "asc")
    {
        var (items, totalCount) = await _boardService.GetBoardsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    // GET: Boards/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var board = await _boardService.GetBoardByIdAsync(id.Value);
        if (board == null)
        {
            return NotFound();
        }

        return View(board);
    }

    // GET: Boards/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Boards/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,CountryId,BoardName,Remarks,IsActive")] Board board)
    {
        if (ModelState.IsValid)
        {
            await _boardService.CreateBoardAsync(board);
            return RedirectToAction(nameof(Index));
        }
        return View(board);
    }

    // GET: Boards/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var board = await _boardService.GetBoardByIdAsync(id.Value);
        if (board == null)
        {
            return NotFound();
        }
        return View(board);
    }

    // POST: Boards/Edit/5
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
                await _boardService.UpdateBoardAsync(board);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _boardService.BoardExistsAsync(board.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(board);
    }

    // GET: Boards/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var board = await _boardService.GetBoardByIdAsync(id.Value);
        if (board == null)
        {
            return NotFound();
        }

        return View(board);
    }

    // POST: Boards/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _boardService.DeleteBoardAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
