using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
public class SmtpConfigurationsController : Controller
    {
        private readonly AppDbContext _context;

        public SmtpConfigurationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SmtpConfigurations1 with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Host", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.SmtpConfigurations.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.Host.Contains(search) ||
                    s.From.Contains(search) ||
                    s.UserName.Contains(search) ||
                    s.Port.ToString().Contains(search)
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

        private static System.Linq.Expressions.Expression<Func<SmtpConfiguration, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "host" => s => s.Host,
                "from" => s => s.From,
                "port" => s => s.Port,
                "username" => s => s.UserName,
                "enablessl" => s => s.EnableSsl,
                _ => s => s.Host
            };
        }

        // Helper to get filtered items for export (with pagination)
        private async Task<(List<SmtpConfiguration> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.SmtpConfigurations.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.Host.Contains(search) ||
                    s.From.Contains(search) ||
                    s.UserName.Contains(search) ||
                    s.Port.ToString().Contains(search)
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
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "Host", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("Host,From Email,Port,Username,Enable SSL");

            foreach (var s in items)
            {
                // Mask password for security
                var maskedPassword = string.IsNullOrEmpty(s.Password) ? "" : new string('*', s.Password.Length);

                sb.AppendLine($"{EscapeCsv(s.Host)}," +
                              $"{EscapeCsv(s.From)}," +
                              $"{s.Port}," +
                              $"{EscapeCsv(s.UserName)}," +
                              $"{(s.EnableSsl ? "Yes" : "No")}");
            }

            var fileName = $"SMTPConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", fileName);
        }

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Host", string sortDir = "asc")
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

        // GET: SmtpConfigurations1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var smtpConfiguration = await _context.SmtpConfigurations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (smtpConfiguration == null)
            {
                return NotFound();
            }

            return View(smtpConfiguration);
        }

        // GET: SmtpConfigurations1/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SmtpConfigurations1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Host,From,Port,UserName,Password,EnableSsl")] SmtpConfiguration smtpConfiguration)
        {
            if (ModelState.IsValid)
            {
                _context.Add(smtpConfiguration);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(smtpConfiguration);
        }

        // GET: SmtpConfigurations1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var smtpConfiguration = await _context.SmtpConfigurations.FindAsync(id);
            if (smtpConfiguration == null)
            {
                return NotFound();
            }
            return View(smtpConfiguration);
        }

        // POST: SmtpConfigurations1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Host,From,Port,UserName,Password,EnableSsl")] SmtpConfiguration smtpConfiguration)
        {
            if (id != smtpConfiguration.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(smtpConfiguration);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SmtpConfigurationExists(smtpConfiguration.Id))
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
            return View(smtpConfiguration);
        }

        // GET: SmtpConfigurations1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var smtpConfiguration = await _context.SmtpConfigurations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (smtpConfiguration == null)
            {
                return NotFound();
            }

            return View(smtpConfiguration);
        }

        // POST: SmtpConfigurations1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var smtpConfiguration = await _context.SmtpConfigurations.FindAsync(id);
            if (smtpConfiguration != null)
            {
                _context.SmtpConfigurations.Remove(smtpConfiguration);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SmtpConfigurationExists(int id)
        {
            return _context.SmtpConfigurations.Any(e => e.Id == id);
        }
    }