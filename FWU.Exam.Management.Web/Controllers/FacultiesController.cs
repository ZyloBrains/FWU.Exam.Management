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
    public class FacultiesController : Controller
    {
        private readonly AppDbContext _context;

        public FacultiesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Faculties with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "FacultyName", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.Faculties.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f =>
                    f.FacultyCode.Contains(search) ||
                    f.FacultyName.Contains(search) ||
                    (f.ShortName != null && f.ShortName.Contains(search)) ||
                    (f.Remarks != null && f.Remarks.Contains(search))
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

        private static System.Linq.Expressions.Expression<Func<Faculty, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "facultycode" => f => f.FacultyCode,
                "facultyname" => f => f.FacultyName,
                "shortname" => f => f.ShortName,
                "remarks" => f => f.Remarks,
                "isactive" => f => f.IsActive,
                _ => f => f.FacultyName
            };
        }

        // Helper to get filtered items for export
        private async Task<(List<Faculty> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.Faculties.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f =>
                    f.FacultyCode.Contains(search) ||
                    f.FacultyName.Contains(search) ||
                    (f.ShortName != null && f.ShortName.Contains(search)) ||
                    (f.Remarks != null && f.Remarks.Contains(search))
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
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "FacultyName", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("Faculty Code,Faculty Name,Short Name,Remarks,Status");

            foreach (var f in items)
            {
                sb.AppendLine($"{EscapeCsv(f.FacultyCode)}," +
                              $"{EscapeCsv(f.FacultyName)}," +
                              $"{EscapeCsv(f.ShortName)}," +
                              $"{EscapeCsv(f.Remarks)}," +
                              $"{(f.IsActive ? "Active" : "Inactive")}");
            }

            var fileName = $"Faculties_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", fileName);
        }

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "FacultyName", string sortDir = "asc")
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

        // GET: Faculties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(m => m.Id == id);
            if (faculty == null)
            {
                return NotFound();
            }

            return View(faculty);
        }

        // GET: Faculties/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Faculties/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FacultyCode,FacultyName,ShortName,Remarks,IsActive")] Faculty faculty)
        {
            if (ModelState.IsValid)
            {
                _context.Add(faculty);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(faculty);
        }

        // GET: Faculties/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null)
            {
                return NotFound();
            }
            return View(faculty);
        }

        // POST: Faculties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FacultyCode,FacultyName,ShortName,Remarks,IsActive")] Faculty faculty)
        {
            if (id != faculty.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(faculty);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacultyExists(faculty.Id))
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
            return View(faculty);
        }

        // GET: Faculties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var faculty = await _context.Faculties
                .FirstOrDefaultAsync(m => m.Id == id);
            if (faculty == null)
            {
                return NotFound();
            }

            return View(faculty);
        }

        // POST: Faculties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty != null)
            {
                _context.Faculties.Remove(faculty);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FacultyExists(int id)
        {
            return _context.Faculties.Any(e => e.Id == id);
        }
    }
}