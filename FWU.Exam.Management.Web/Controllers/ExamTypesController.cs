using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Web.Controllers
{
    public class ExamTypesController : Controller
    {
        private readonly AppDbContext _context;

        public ExamTypesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ExamTypes with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Name", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.ExamTypes.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    //e.Code.Contains(search) ||
                    e.Name.Contains(search) ||
                    (e.Remarks != null && e.Remarks.Contains(search))
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

        private static System.Linq.Expressions.Expression<Func<ExamType, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "code" => e => e.Code,
                "name" => e => e.Name,
                "remarks" => e => e.Remarks,
                "isactive" => e => e.IsActive,
                _ => e => e.Name
            };
        }

        // Helper to get filtered items for export
        private async Task<(List<ExamType> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.ExamTypes.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    //e.Code.Contains(search) ||
                    e.Name.Contains(search) ||
                    (e.Remarks != null && e.Remarks.Contains(search))
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
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("Code,Name,Remarks,Status");

            foreach (var e in items)
            {
                sb.AppendLine(
                       //$"{EscapeCsv(e.Code)}," +
                              $"{EscapeCsv(e.Name)}," +
                              $"{EscapeCsv(e.Remarks)}," +
                              $"{(e.IsActive ? "Active" : "Inactive")}");
            }

            var fileName = $"ExamTypes_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", fileName);
        }

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
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

        // GET: ExamTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examType = await _context.ExamTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (examType == null)
            {
                return NotFound();
            }

            return View(examType);
        }

        // GET: ExamTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ExamTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Remarks,IsActive,Code")] ExamType examType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(examType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(examType);
        }

        // GET: ExamTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examType = await _context.ExamTypes.FindAsync(id);
            if (examType == null)
            {
                return NotFound();
            }
            return View(examType);
        }

        // POST: ExamTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Remarks,IsActive,Code")] ExamType examType)
        {
            if (id != examType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(examType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExamTypeExists(examType.Id))
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
            return View(examType);
        }

        // GET: ExamTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examType = await _context.ExamTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (examType == null)
            {
                return NotFound();
            }

            return View(examType);
        }

        // POST: ExamTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var examType = await _context.ExamTypes.FindAsync(id);
            if (examType != null)
            {
                _context.ExamTypes.Remove(examType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExamTypeExists(int id)
        {
            return _context.ExamTypes.Any(e => e.Id == id);
        }
    }
}