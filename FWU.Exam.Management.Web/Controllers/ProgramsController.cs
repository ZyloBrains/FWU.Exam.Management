using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Web.Controllers
{
    public class ProgramsController : Controller
    {
        private readonly AppDbContext _context;

        public ProgramsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Programs1 with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "ProgramCode", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.Programs
                .Include(p => p.Board)
                .Include(p => p.Faculty)
                .Include(p => p.Level)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.ProgramCode.Contains(search) ||
                    p.ProgramName.Contains(search) ||
                    p.ShortName.Contains(search) ||
                    p.Remarks.Contains(search) ||
                    (p.Level != null && p.Level.LevelName.Contains(search)) ||
                    (p.Faculty != null && p.Faculty.FacultyCode.Contains(search)) ||
                    (p.Board != null && p.Board.BoardName.Contains(search))
                );
            }

            // Apply sorting
            query = sortDir.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sort))
                : query.OrderBy(GetSortProperty(sort));

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.SortDir = sortDir;

            return View(items);
        }

        private static System.Linq.Expressions.Expression<Func<Program, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "programcode" => p => p.ProgramCode,
                "programname" => p => p.ProgramName,
                "shortname" => p => p.ShortName,
                "level" => p => p.Level.LevelName,
                "faculty" => p => p.Faculty.FacultyCode,
                "board" => p => p.Board.BoardName,
                "duration" => p => p.Duration,
                "grandtotalmarks" => p => p.GrandTotalMarks,
                "numberofseats" => p => p.NumberOfSeats,
                "isactive" => p => p.IsActive,
                _ => p => p.ProgramCode
            };
        }

        // Helper to get filtered items for export (with pagination)
        private async Task<(List<Program> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.Programs
                .Include(p => p.Board)
                .Include(p => p.Faculty)
                .Include(p => p.Level)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.ProgramCode.Contains(search) ||
                    p.ProgramName.Contains(search) ||
                    p.ShortName.Contains(search) ||
                    p.Remarks.Contains(search) ||
                    (p.Level != null && p.Level.LevelName.Contains(search)) ||
                    (p.Faculty != null && p.Faculty.FacultyCode.Contains(search)) ||
                    (p.Board != null && p.Board.BoardName.Contains(search))
                );
            }

            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortDir.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sort))
                : query.OrderBy(GetSortProperty(sort));

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
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
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "ProgramCode", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("Program Code,Program Name,Short Name,Level,Faculty,Board,Program Period Type,Duration,Grand Total Marks,Has Multiple Intakes,Number of Seats,Scholarship Seats,Roll Number Prefix,Remarks,Status");

            foreach (var p in items)
            {
                sb.AppendLine($"{EscapeCsv(p.ProgramCode)}," +
                               $"{EscapeCsv(p.ProgramName)}," +
                               $"{EscapeCsv(p.ShortName)}," +
                               $"{EscapeCsv(p.Level?.LevelName)}," +
                               $"{EscapeCsv(p.Faculty?.FacultyCode)}," +
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

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "ProgramCode", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.SortDir = sortDir;

            return View("PrintPdf", items);
        }

        // GET: Programs1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var program = await _context.Programs
                .Include(p => p.Board)
                .Include(p => p.Faculty)
                .Include(p => p.Level)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        // GET: Programs1/Create
        public IActionResult Create()
        {
            ViewData["BoardId"] = new SelectList(_context.Boards, "Id", "BoardName");
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "Id", "FacultyCode");
            ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName");
            return View();
        }

        // POST: Programs1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LevelId,FacultyId,BoardId,ProgramCode,ProgramName,ShortName,Duration,GrandTotalMarks,HasMultipleIntakes,NumberOfSeats,ScholarshipSeats,Remarks,IsActive,RollNumberPrefix")] Program program)
        {
            if (ModelState.IsValid)
            {
                _context.Add(program);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BoardId"] = new SelectList(_context.Boards, "Id", "BoardName", program.BoardId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "Id", "FacultyCode", program.FacultyId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName", program.LevelId);
            return View(program);
        }

        // GET: Programs1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var program = await _context.Programs.FindAsync(id);
            if (program == null)
            {
                return NotFound();
            }
            ViewData["BoardId"] = new SelectList(_context.Boards, "Id", "BoardName", program.BoardId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "Id", "FacultyCode", program.FacultyId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName", program.LevelId);
            return View(program);
        }

        // POST: Programs1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LevelId,FacultyId,BoardId,ProgramCode,ProgramName,ShortName,Duration,GrandTotalMarks,HasMultipleIntakes,NumberOfSeats,ScholarshipSeats,Remarks,IsActive,RollNumberPrefix")] Program program)
        {
            if (id != program.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(program);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProgramExists(program.Id))
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
            ViewData["BoardId"] = new SelectList(_context.Boards, "Id", "BoardName", program.BoardId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "Id", "FacultyCode", program.FacultyId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName", program.LevelId);
            return View(program);
        }

        // GET: Programs1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var program = await _context.Programs
                .Include(p => p.Board)
                .Include(p => p.Faculty)
                .Include(p => p.Level)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (program == null)
            {
                return NotFound();
            }

            return View(program);
        }

        // POST: Programs1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var program = await _context.Programs.FindAsync(id);
            if (program != null)
            {
                _context.Programs.Remove(program);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProgramExists(int id)
        {
            return _context.Programs.Any(e => e.Id == id);
        }
    }
}