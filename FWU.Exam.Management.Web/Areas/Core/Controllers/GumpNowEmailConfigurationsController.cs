using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("gumpnowemail.view")]
public class GumpNowEmailConfigurationsController(AppDbContext context, IAuditLogWriter auditLogWriter) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "ApiUrl", string sortDir = "asc", int pageSize = 10)
    {
        var query = context.GumpNowEmailConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.ApiUrl.Contains(search) ||
                s.FromAddr.Contains(search) ||
                (s.Mode ?? "").Contains(search)
            );
        }

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

    private static System.Linq.Expressions.Expression<Func<GumpNowEmailConfiguration, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "apiurl" => s => s.ApiUrl,
            "fromaddr" => s => s.FromAddr,
            "mode" => s => s.Mode ?? "",
            "overrideunsubscription" => s => s.OverrideUnsubscription,
            "isactive" => s => s.IsActive,
            _ => s => s.Id
        };
    }

    private async Task<(List<GumpNowEmailConfiguration> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.GumpNowEmailConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.ApiUrl.Contains(search) ||
                s.FromAddr.Contains(search) ||
                (s.Mode ?? "").Contains(search)
            );
        }

        var totalCount = await query.CountAsync();

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }


    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "ApiUrl", string sortDir = "asc")
    {
        var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("API URL,From Address,Mode,Override Unsubscription,Active");

        foreach (var s in items)
        {
            sb.AppendLine($"{s.ApiUrl.EscapeCsv()}," +
                          $"{s.FromAddr.EscapeCsv()}," +
                          $"{s.Mode.EscapeCsv()}," +
                          $"{(s.OverrideUnsubscription ? "Yes" : "No")}," +
                          $"{(s.IsActive ? "Yes" : "No")}");
        }

        var fileName = $"GumpNowEmailConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "ApiUrl", string sortDir = "asc")
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
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "ApiUrl", string sortDir = "asc")
    {
        var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("GumpNow Email Configurations");

        var headers = new[] { "API URL", "From Address", "Mode", "Override Unsubscription", "Active" };
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
            worksheet.Cell(row, 1).Value = s.ApiUrl;
            worksheet.Cell(row, 2).Value = s.FromAddr;
            worksheet.Cell(row, 3).Value = s.Mode;
            worksheet.Cell(row, 4).Value = s.OverrideUnsubscription ? "Yes" : "No";
            worksheet.Cell(row, 5).Value = s.IsActive ? "Yes" : "No";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"GumpNowEmailConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [RequirePermission("gumpnowemail.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("gumpnowemail.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ApiUrl,ApiKey,FromAddr,Mode,OverrideUnsubscription,IsActive")] GumpNowEmailConfiguration config)
    {
        if (ModelState.IsValid)
        {
            context.Add(config);
            await context.SaveChangesAsync();
            await auditLogWriter.LogAsync(ActivityTypes.EmailConfigUpdated, $"GumpNow email configuration created (id {config.Id})", new { id = config.Id, apiUrl = config.ApiUrl, fromAddr = config.FromAddr, isActive = config.IsActive }, entityName: "GumpNowEmailConfiguration", entityId: config.Id.ToString());
            TempData["SuccessMessage"] = "GumpNow email configuration created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(config);
    }

    [RequirePermission("gumpnowemail.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var config = await context.GumpNowEmailConfigurations.FindAsync(id);
        if (config == null) return NotFound();

        return View(config);
    }

    [RequirePermission("gumpnowemail.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ApiUrl,ApiKey,FromAddr,Mode,OverrideUnsubscription,IsActive")] GumpNowEmailConfiguration config)
    {
        if (id != config.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                context.Update(config);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GumpNowEmailConfigurationExists(config.Id)) return NotFound();
                throw;
            }
            await auditLogWriter.LogAsync(ActivityTypes.EmailConfigUpdated, $"GumpNow email configuration {config.Id} updated", new { id = config.Id, apiUrl = config.ApiUrl, fromAddr = config.FromAddr, isActive = config.IsActive }, entityName: "GumpNowEmailConfiguration", entityId: config.Id.ToString());
            TempData["SuccessMessage"] = "GumpNow email configuration updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(config);
    }

    [RequirePermission("gumpnowemail.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var config = await context.GumpNowEmailConfigurations.FirstOrDefaultAsync(m => m.Id == id);
        if (config == null) return NotFound();

        return View(config);
    }

    [RequirePermission("gumpnowemail.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var config = await context.GumpNowEmailConfigurations.FindAsync(id);
            if (config != null)
            {
                context.GumpNowEmailConfigurations.Remove(config);
            }
            await context.SaveChangesAsync();
            await auditLogWriter.LogAsync(ActivityTypes.EmailConfigUpdated, $"GumpNow email configuration {id} deleted", new { id }, entityName: "GumpNowEmailConfiguration", entityId: id.ToString());
            TempData["SuccessMessage"] = "GumpNow email configuration deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
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

    [RequirePermission("gumpnowemail.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            var entity = await context.GumpNowEmailConfigurations.FindAsync(id);
            if (entity != null)
            {
                context.GumpNowEmailConfigurations.Remove(entity);
                await context.SaveChangesAsync();
                await auditLogWriter.LogAsync(ActivityTypes.EmailConfigUpdated, $"GumpNow email configuration {id} deleted", new { id }, entityName: "GumpNowEmailConfiguration", entityId: id.ToString());
            }
            return Json(new { success = true, message = "GumpNow email configuration deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private bool GumpNowEmailConfigurationExists(int id)
    {
        return context.GumpNowEmailConfigurations.Any(e => e.Id == id);
    }
}
