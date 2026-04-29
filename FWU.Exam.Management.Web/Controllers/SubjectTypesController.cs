using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models.Subjects;

namespace fwu_examination_management_system.Controllers
{
    public class SubjectTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SubjectTypes with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "SubjectTypeName", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.SubjectTypes.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.SubjectTypeName.Contains(search) ||
                    (s.Remarks != null && s.Remarks.Contains(search))
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

        private static System.Linq.Expressions.Expression<Func<SubjectType, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "subjecttypename" => s => s.SubjectTypeName,
                "remarks" => s => s.Remarks,
                "maxallowedsubjects" => s => s.MaxAllowedSubjects,
                "isdefault" => s => s.IsDefault,
                "isactive" => s => s.IsActive,
                _ => s => s.SubjectTypeName
            };
        }

        // Helper to get filtered items for export
        private async Task<(List<SubjectType> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.SubjectTypes.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.SubjectTypeName.Contains(search) ||
                    (s.Remarks != null && s.Remarks.Contains(search))
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
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "SubjectTypeName", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("Subject Type Name,Remarks,Max Allowed Subjects,Is Default,Status");

            foreach (var s in items)
            {
                sb.AppendLine($"{EscapeCsv(s.SubjectTypeName)}," +
                              $"{EscapeCsv(s.Remarks)}," +
                              $"{s.MaxAllowedSubjects}," +
                              $"{(s.IsDefault ? "Yes" : "No")}," +
                              $"{(s.IsActive ? "Active" : "Inactive")}");
            }

            var fileName = $"SubjectTypes_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", fileName);
        }

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "SubjectTypeName", string sortDir = "asc")
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

        // GET: SubjectTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectType = await _context.SubjectTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subjectType == null)
            {
                return NotFound();
            }

            return View(subjectType);
        }

        // GET: SubjectTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SubjectTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SubjectTypeName,Remarks,IsActive,IsDefault,MaxAllowedSubjects")] SubjectType subjectType)
        {
            if (ModelState.IsValid)
            {
                // If this is set as default, remove default from other records
                if (subjectType.IsDefault)
                {
                    var existingDefault = await _context.SubjectTypes.FirstOrDefaultAsync(s => s.IsDefault);
                    if (existingDefault != null)
                    {
                        existingDefault.IsDefault = false;
                        _context.Update(existingDefault);
                    }
                }

                _context.Add(subjectType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subjectType);
        }

        // GET: SubjectTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectType = await _context.SubjectTypes.FindAsync(id);
            if (subjectType == null)
            {
                return NotFound();
            }
            return View(subjectType);
        }

        // POST: SubjectTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SubjectTypeName,Remarks,IsActive,IsDefault,MaxAllowedSubjects")] SubjectType subjectType)
        {
            if (id != subjectType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // If this is set as default, remove default from other records
                    if (subjectType.IsDefault)
                    {
                        var existingDefault = await _context.SubjectTypes
                            .FirstOrDefaultAsync(s => s.IsDefault && s.Id != subjectType.Id);
                        if (existingDefault != null)
                        {
                            existingDefault.IsDefault = false;
                            _context.Update(existingDefault);
                        }
                    }

                    _context.Update(subjectType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectTypeExists(subjectType.Id))
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
            return View(subjectType);
        }

        // GET: SubjectTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectType = await _context.SubjectTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subjectType == null)
            {
                return NotFound();
            }

            return View(subjectType);
        }

        // POST: SubjectTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subjectType = await _context.SubjectTypes.FindAsync(id);
            if (subjectType != null)
            {
                _context.SubjectTypes.Remove(subjectType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectTypeExists(int id)
        {
            return _context.SubjectTypes.Any(e => e.Id == id);
        }
    }
}