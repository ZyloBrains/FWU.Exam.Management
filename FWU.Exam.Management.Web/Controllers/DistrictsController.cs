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
    public class DistrictsController : Controller
    {
        private readonly AppDbContext _context;

        public DistrictsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Districts with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "DistrictName", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.Districts
                .Include(d => d.Province)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d =>
                    d.DistrictCode.Contains(search) ||
                    d.DistrictName.Contains(search) ||
                    (d.Province != null && d.Province.ProvinceName.Contains(search))
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

        private static System.Linq.Expressions.Expression<Func<District, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "districtcode" => d => d.DistrictCode,
                "districtname" => d => d.DistrictName,
                "provincename" => d => d.Province.ProvinceName,
                _ => d => d.DistrictName
            };
        }

        // Helper to get filtered items for export
        private async Task<(List<District> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.Districts
                .Include(d => d.Province)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d =>
                    d.DistrictCode.Contains(search) ||
                    d.DistrictName.Contains(search) ||
                    (d.Province != null && d.Province.ProvinceName.Contains(search))
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
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "DistrictName", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("District Code,District Name,Province");

            foreach (var d in items)
            {
                sb.AppendLine($"{EscapeCsv(d.DistrictCode)}," +
                              $"{EscapeCsv(d.DistrictName)}," +
                              $"{EscapeCsv(d.Province?.ProvinceName ?? "")}");
            }

            var fileName = $"Districts_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", fileName);
        }

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "DistrictName", string sortDir = "asc")
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

        // GET: Districts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _context.Districts
                .Include(d => d.Province)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (district == null)
            {
                return NotFound();
            }

            return View(district);
        }

        // GET: Districts/Create
        public IActionResult Create()
        {
            ViewData["ProvinceId"] = new SelectList(_context.Provinces.Where(p => p.IsActive), "Id", "ProvinceName");
            return View();
        }

        // POST: Districts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProvinceId,DistrictCode,DistrictName")] District district)
        {
            if (ModelState.IsValid)
            {
                _context.Add(district);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProvinceId"] = new SelectList(_context.Provinces.Where(p => p.IsActive), "Id", "ProvinceName", district.ProvinceId);
            return View(district);
        }

        // GET: Districts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _context.Districts.FindAsync(id);
            if (district == null)
            {
                return NotFound();
            }
            ViewData["ProvinceId"] = new SelectList(_context.Provinces.Where(p => p.IsActive), "Id", "ProvinceName", district.ProvinceId);
            return View(district);
        }

        // POST: Districts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProvinceId,DistrictCode,DistrictName")] District district)
        {
            if (id != district.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(district);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DistrictExists(district.Id))
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
            ViewData["ProvinceId"] = new SelectList(_context.Provinces.Where(p => p.IsActive), "Id", "ProvinceName", district.ProvinceId);
            return View(district);
        }

        // GET: Districts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var district = await _context.Districts
                .Include(d => d.Province)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (district == null)
            {
                return NotFound();
            }

            return View(district);
        }

        // POST: Districts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var district = await _context.Districts.FindAsync(id);
            if (district != null)
            {
                _context.Districts.Remove(district);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DistrictExists(int id)
        {
            return _context.Districts.Any(e => e.Id == id);
        }
    }
}