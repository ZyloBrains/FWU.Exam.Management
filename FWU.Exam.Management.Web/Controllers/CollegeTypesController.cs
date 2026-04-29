using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models.Colleges;

namespace fwu_examination_management_system.Controllers
{
    public class CollegeTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CollegeTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CollegeTypes with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Name", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.CollegeTypes.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Code.Contains(search) ||
                    c.Name.Contains(search) ||
                    (c.Remarks != null && c.Remarks.Contains(search))
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

        private static System.Linq.Expressions.Expression<Func<CollegeType, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "code" => c => c.Code,
                "name" => c => c.Name,
                "remarks" => c => c.Remarks,
                "isdefault" => c => c.IsDefault,
                "isactive" => c => c.IsActive,
                _ => c => c.Name
            };
        }

        // Helper to get filtered items for export
        private async Task<(List<CollegeType> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.CollegeTypes.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Code.Contains(search) ||
                    c.Name.Contains(search) ||
                    (c.Remarks != null && c.Remarks.Contains(search))
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

        // Helper method to handle default CollegeType
        private async Task HandleDefaultCollegeType(CollegeType collegeType)
        {
            if (collegeType.IsDefault)
            {
                var existingDefault = await _context.CollegeTypes
                    .FirstOrDefaultAsync(c => c.IsDefault && c.Id != collegeType.Id);
                if (existingDefault != null)
                {
                    existingDefault.IsDefault = false;
                    _context.Update(existingDefault);
                }
            }
        }

        // Export to CSV (Current Page with pagination)
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("Code,Name,Remarks,Is Default,Status");

            foreach (var c in items)
            {
                sb.AppendLine($"{EscapeCsv(c.Code)}," +
                              $"{EscapeCsv(c.Name)}," +
                              $"{EscapeCsv(c.Remarks ?? "N/A")}," +
                              $"{(c.IsDefault ? "Yes" : "No")}," +
                              $"{(c.IsActive ? "Active" : "Inactive")}");
            }

            var fileName = $"CollegeTypes_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
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

        // GET: CollegeTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collegeType = await _context.CollegeTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (collegeType == null)
            {
                return NotFound();
            }

            return View(collegeType);
        }

        // GET: CollegeTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CollegeTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Name,Remarks,IsDefault,IsActive")] CollegeType collegeType)
        {
            if (ModelState.IsValid)
            {
                await HandleDefaultCollegeType(collegeType);
                _context.Add(collegeType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(collegeType);
        }

        // GET: CollegeTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collegeType = await _context.CollegeTypes.FindAsync(id);
            if (collegeType == null)
            {
                return NotFound();
            }
            return View(collegeType);
        }

        // POST: CollegeTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,Remarks,IsDefault,IsActive")] CollegeType collegeType)
        {
            if (id != collegeType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await HandleDefaultCollegeType(collegeType);
                    _context.Update(collegeType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CollegeTypeExists(collegeType.Id))
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
            return View(collegeType);
        }

        // GET: CollegeTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collegeType = await _context.CollegeTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (collegeType == null)
            {
                return NotFound();
            }

            return View(collegeType);
        }

        // POST: CollegeTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var collegeType = await _context.CollegeTypes.FindAsync(id);
            if (collegeType != null)
            {
                _context.CollegeTypes.Remove(collegeType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CollegeTypeExists(int id)
        {
            return _context.CollegeTypes.Any(e => e.Id == id);
        }
    }
}