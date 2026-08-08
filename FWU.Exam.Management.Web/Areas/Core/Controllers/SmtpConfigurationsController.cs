using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Extensions;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("smtp.view")]
public class SmtpConfigurationsController(AppDbContext context) : Controller
    {

    // GET: SmtpConfigurations1 with pagination, search, and sorting
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Host", string sortDir = "asc", int pageSize = 10)
        {
            var query = context.SmtpConfigurations.AsNoTracking();

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
                "isactive" => s => s.IsActive,
                _ => s => s.Host
            };
        }

        // Helper to get filtered items for export (with pagination)
        private async Task<(List<SmtpConfiguration> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string? search, string sort, string sortDir)
        {
            var query = context.SmtpConfigurations.AsNoTracking();

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

        // Export to CSV (Current Page with pagination)
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "Host", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("Host,From Email,Port,Username,Enable SSL,Is Active");

            foreach (var s in items)
            {
                // Mask password for security
                var maskedPassword = string.IsNullOrEmpty(s.Password) ? "" : new string('*', s.Password.Length);

                sb.AppendLine($"{s.Host.EscapeCsv()}," +
                              $"{s.From.EscapeCsv()}," +
                              $"{s.Port}," +
                              $"{s.UserName.EscapeCsv()}," +
                              $"{(s.EnableSsl ? "Yes" : "No")}," +
                              $"{(s.IsActive ? "Yes" : "No")}");
            }

            var fileName = $"SMTPConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", fileName);
        }

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "Host", string sortDir = "asc")
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

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "Host", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("SMTP Configurations");

            var headers = new[] { "Host", "From Email", "Port", "Username", "Enable SSL", "Is Active" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            int row = 2;
            foreach (var s in items)
            {
                worksheet.Cell(row, 1).Value = s.Host;
                worksheet.Cell(row, 2).Value = s.From;
                worksheet.Cell(row, 3).Value = s.Port;
                worksheet.Cell(row, 4).Value = s.UserName;
                worksheet.Cell(row, 5).Value = s.EnableSsl ? "Yes" : "No";
                worksheet.Cell(row, 6).Value = s.IsActive ? "Yes" : "No";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var fileName = $"SMTPConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET: SmtpConfigurations1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var smtpConfiguration = await context.SmtpConfigurations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (smtpConfiguration == null)
            {
                return NotFound();
            }

            return View(smtpConfiguration);
        }

        // GET: SmtpConfigurations1/Create
        [RequirePermission("smtp.create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: SmtpConfigurations1/Create
        [RequirePermission("smtp.create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Host,From,Port,UserName,Password,EnableSsl,IsActive")] SmtpConfiguration smtpConfiguration)
        {
            if (ModelState.IsValid)
            {
                context.Add(smtpConfiguration);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "SMTP configuration created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(smtpConfiguration);
        }

        // GET: SmtpConfigurations1/Edit/5
        [RequirePermission("smtp.edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var smtpConfiguration = await context.SmtpConfigurations.FindAsync(id);
            if (smtpConfiguration == null)
            {
                return NotFound();
            }
            return View(smtpConfiguration);
        }

        // POST: SmtpConfigurations1/Edit/5
        [RequirePermission("smtp.edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Host,From,Port,UserName,Password,EnableSsl,IsActive")] SmtpConfiguration smtpConfiguration)
        {
            if (id != smtpConfiguration.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(smtpConfiguration);
                    await context.SaveChangesAsync();
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
                TempData["SuccessMessage"] = "SMTP configuration updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(smtpConfiguration);
        }

        // GET: SmtpConfigurations1/Delete/5
        [RequirePermission("smtp.delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var smtpConfiguration = await context.SmtpConfigurations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (smtpConfiguration == null)
            {
                return NotFound();
            }

            return View(smtpConfiguration);
        }

        // POST: SmtpConfigurations1/Delete/5
        [RequirePermission("smtp.delete")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var smtpConfiguration = await context.SmtpConfigurations.FindAsync(id);
                if (smtpConfiguration != null)
                {
                    context.SmtpConfigurations.Remove(smtpConfiguration);
                }

                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "SMTP configuration deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                TempData["ErrorMessage"] = "Cannot delete this record because it is referenced by other records. Please remove or reassign dependent records first.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("smtp.delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var entity = await context.SmtpConfigurations.FindAsync(id);
                if (entity != null)
                {
                    context.SmtpConfigurations.Remove(entity);
                    await context.SaveChangesAsync();
                }
                return Json(new { success = true, message = "SMTP configuration deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool SmtpConfigurationExists(int id)
        {
            return context.SmtpConfigurations.Any(e => e.Id == id);
        }
    }