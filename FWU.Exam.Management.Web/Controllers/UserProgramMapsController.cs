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
    public class UserProgramMapsController : Controller
    {
        private readonly AppDbContext _context;

        public UserProgramMapsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: UserProgramMaps1 with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "User", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.UserProgramMaps
                .Include(u => u.Program)
                .Include(u => u.User)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.User.UserName.Contains(search) ||
                    u.User.Email.Contains(search) ||
                    u.Program.ProgramCode.Contains(search) ||
                    u.Program.ProgramName.Contains(search)
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

        private static System.Linq.Expressions.Expression<Func<UserProgramMap, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "user" => u => u.User.UserName,
                "email" => u => u.User.Email,
                "program" => u => u.Program.ProgramCode,
                "programname" => u => u.Program.ProgramName,
                _ => u => u.User.UserName
            };
        }

        // Helper to get filtered items for export (with pagination)
        private async Task<(List<UserProgramMap> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.UserProgramMaps
                .Include(u => u.Program)
                .Include(u => u.User)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.User.UserName.Contains(search) ||
                    u.User.Email.Contains(search) ||
                    u.Program.ProgramCode.Contains(search) ||
                    u.Program.ProgramName.Contains(search)
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
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "User", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("Username,Email,Program Code,Program Name");

            foreach (var u in items)
            {
                sb.AppendLine($"{EscapeCsv(u.User?.UserName)}," +
                              $"{EscapeCsv(u.User?.Email)}," +
                              $"{EscapeCsv(u.Program?.ProgramCode)}," +
                              $"{EscapeCsv(u.Program?.ProgramName)}");
            }

            var fileName = $"UserProgramMaps_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", fileName);
        }

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "User", string sortDir = "asc")
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

        // GET: UserProgramMaps1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProgramMap = await _context.UserProgramMaps
                .Include(u => u.Program)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userProgramMap == null)
            {
                return NotFound();
            }

            return View(userProgramMap);
        }

        // GET: UserProgramMaps1/Create
        public IActionResult Create()
        {
            ViewData["ProgramId"] = new SelectList(_context.Programs, "Id", "ProgramCode", "ProgramName");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", "Email");
            return View();
        }

        // POST: UserProgramMaps1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,ProgramId")] UserProgramMap userProgramMap)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userProgramMap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProgramId"] = new SelectList(_context.Programs, "Id", "ProgramCode", userProgramMap.ProgramId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", userProgramMap.UserId);
            return View(userProgramMap);
        }

        // GET: UserProgramMaps1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProgramMap = await _context.UserProgramMaps.FindAsync(id);
            if (userProgramMap == null)
            {
                return NotFound();
            }
            ViewData["ProgramId"] = new SelectList(_context.Programs, "Id", "ProgramCode", userProgramMap.ProgramId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", userProgramMap.UserId);
            return View(userProgramMap);
        }

        // POST: UserProgramMaps1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,ProgramId")] UserProgramMap userProgramMap)
        {
            if (id != userProgramMap.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userProgramMap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserProgramMapExists(userProgramMap.Id))
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
            ViewData["ProgramId"] = new SelectList(_context.Programs, "Id", "ProgramCode", userProgramMap.ProgramId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", userProgramMap.UserId);
            return View(userProgramMap);
        }

        // GET: UserProgramMaps1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userProgramMap = await _context.UserProgramMaps
                .Include(u => u.Program)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userProgramMap == null)
            {
                return NotFound();
            }

            return View(userProgramMap);
        }

        // POST: UserProgramMaps1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userProgramMap = await _context.UserProgramMaps.FindAsync(id);
            if (userProgramMap != null)
            {
                _context.UserProgramMaps.Remove(userProgramMap);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserProgramMapExists(int id)
        {
            return _context.UserProgramMaps.Any(e => e.Id == id);
        }
    }
}