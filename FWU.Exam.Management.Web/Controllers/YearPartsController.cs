using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models;

namespace fwu_examination_management_system.Controllers
{
    public class YearPartsController : Controller
    {
        private readonly AppDbContext _context;

        public YearPartsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: YearParts with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Year", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.YearParts
                .Include(y => y.ProgramPeriodType)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(y =>
                    y.YearPartName.Contains(search) ||
                    y.Code.Contains(search) ||
                    (y.Remark != null && y.Remark.Contains(search)) ||
                    (y.ProgramPeriodType != null && y.ProgramPeriodType.ProgramPeriodTypeName.Contains(search))
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

        private static System.Linq.Expressions.Expression<Func<YearPart, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "code" => y => y.Code,
                "yearpartname" => y => y.YearPartName,
                "year" => y => y.Year,
                "part" => y => y.Part,
                "programperiodtypename" => y => y.ProgramPeriodType.ProgramPeriodTypeName,
                "isactive" => y => y.IsActive,
                "iseditable" => y => y.IsEditable,
                _ => y => y.Year
            };
        }

        // Helper to get filtered items for export
        private async Task<(List<YearPart> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.YearParts
                .Include(y => y.ProgramPeriodType)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(y =>
                    y.YearPartName.Contains(search) ||
                    y.Code.Contains(search) ||
                    (y.Remark != null && y.Remark.Contains(search)) ||
                    (y.ProgramPeriodType != null && y.ProgramPeriodType.ProgramPeriodTypeName.Contains(search))
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
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "Year", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("Code,Year Part Name,Period Type,Year,Part,Remark,Is Editable,Status");

            foreach (var y in items)
            {
                sb.AppendLine($"{EscapeCsv(y.Code)}," +
                              $"{EscapeCsv(y.YearPartName)}," +
                              $"{EscapeCsv(y.ProgramPeriodType?.ProgramPeriodTypeName ?? "")}," +
                              $"{y.Year}," +
                              $"{y.Part}," +
                              $"{EscapeCsv(y.Remark)}," +
                              $"{(y.IsEditable ? "Yes" : "No")}," +
                              $"{(y.IsActive ? "Active" : "Inactive")}");
            }

            var fileName = $"YearParts_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", fileName);
        }

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Year", string sortDir = "asc")
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

        // GET: YearParts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yearPart = await _context.YearParts
                .Include(y => y.ProgramPeriodType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (yearPart == null)
            {
                return NotFound();
            }

            return View(yearPart);
        }

        // GET: YearParts/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ProgramPeriodTypeId"] = new SelectList(await _context.ProgramPeriodTypes.ToListAsync(), "Id", "ProgramPeriodTypeName");
            return View();
        }

        // POST: YearParts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProgramPeriodTypeId,Year,Part,YearPartName,Remark,IsActive,IsEditable,Code")] YearPart yearPart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(yearPart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProgramPeriodTypeId"] = new SelectList(await _context.ProgramPeriodTypes.ToListAsync(), "Id", "ProgramPeriodTypeName", yearPart.ProgramPeriodTypeId);
            return View(yearPart);
        }

        // GET: YearParts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yearPart = await _context.YearParts.FindAsync(id);
            if (yearPart == null)
            {
                return NotFound();
            }
            ViewData["ProgramPeriodTypeId"] = new SelectList(await _context.ProgramPeriodTypes.ToListAsync(), "Id", "ProgramPeriodTypeName", yearPart.ProgramPeriodTypeId);
            return View(yearPart);
        }

        // POST: YearParts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProgramPeriodTypeId,Year,Part,YearPartName,Remark,IsActive,IsEditable,Code")] YearPart yearPart)
        {
            if (id != yearPart.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(yearPart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!YearPartExists(yearPart.Id))
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
            ViewData["ProgramPeriodTypeId"] = new SelectList(await _context.ProgramPeriodTypes.ToListAsync(), "Id", "ProgramPeriodTypeName", yearPart.ProgramPeriodTypeId);
            return View(yearPart);
        }

        // GET: YearParts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yearPart = await _context.YearParts
                .Include(y => y.ProgramPeriodType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (yearPart == null)
            {
                return NotFound();
            }

            return View(yearPart);
        }

        // POST: YearParts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var yearPart = await _context.YearParts.FindAsync(id);
            if (yearPart != null)
            {
                _context.YearParts.Remove(yearPart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool YearPartExists(int id)
        {
            return _context.YearParts.Any(e => e.Id == id);
        }
    }
}