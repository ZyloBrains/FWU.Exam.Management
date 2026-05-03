using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities.Location;

namespace FWU.Exam.Management.Web.Controllers
{
    public class ProvincesController : Controller
    {
        private readonly AppDbContext _context;

        public ProvincesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Provinces with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "ProvinceName", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.Provinces.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProvinceName != null && p.ProvinceName.Contains(search));
            }

            query = sortDir.ToLower() == "desc"
                ? ApplySortDescending(query, sort)
                : ApplySortAscending(query, sort);

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

        private static IQueryable<Province> ApplySortAscending(IQueryable<Province> query, string sort)
        {
            return sort.ToLower() switch
            {
                "isactive" => query.OrderBy(p => p.IsActive),
                _ => query.OrderBy(p => p.ProvinceName)
            };
        }

        private static IQueryable<Province> ApplySortDescending(IQueryable<Province> query, string sort)
        {
            return sort.ToLower() switch
            {
                "isactive" => query.OrderByDescending(p => p.IsActive),
                _ => query.OrderByDescending(p => p.ProvinceName)
            };
        }

        // Helper to get filtered items for export
        private async Task<(List<Province> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.Provinces.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProvinceName != null && p.ProvinceName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            query = sortDir.ToLower() == "desc"
                ? ApplySortDescending(query, sort)
                : ApplySortAscending(query, sort);

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
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "ProvinceName", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();
            sb.AppendLine("Province Name,Status");

            foreach (var p in items)
            {
                sb.AppendLine($"{EscapeCsv(p.ProvinceName ?? string.Empty)}," +
                              $"{(p.IsActive ? "Active" : "Inactive")}");
            }

            var fileName = $"Provinces_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", fileName);
        }

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "ProvinceName", string sortDir = "asc")
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

        // GET: Provinces/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Provinces
                .FirstOrDefaultAsync(m => m.Id == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        // GET: Provinces/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Provinces/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProvinceName,IsActive")] Province province)
        {
            if (ModelState.IsValid)
            {
                _context.Add(province);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(province);
        }

        // GET: Provinces/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Provinces.FindAsync(id);
            if (province == null)
            {
                return NotFound();
            }
            return View(province);
        }

        // POST: Provinces/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProvinceName,IsActive")] Province province)
        {
            if (id != province.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(province);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProvinceExists(province.Id))
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
            return View(province);
        }

        // GET: Provinces/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var province = await _context.Provinces
                .FirstOrDefaultAsync(m => m.Id == id);
            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        // POST: Provinces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var province = await _context.Provinces.FindAsync(id);
            if (province != null)
            {
                _context.Provinces.Remove(province);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProvinceExists(int id)
        {
            return _context.Provinces.Any(e => e.Id == id);
        }
    }
}