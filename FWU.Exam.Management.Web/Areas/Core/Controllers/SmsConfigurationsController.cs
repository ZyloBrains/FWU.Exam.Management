using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("sms.view")]
public class SmsConfigurationsController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "ApiUrl", string sortDir = "asc", int pageSize = 10)
    {
        var query = context.SmsConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.ApiUrl.Contains(search) ||
                s.Mode.Contains(search) ||
                s.Tags.Contains(search)
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

    private static System.Linq.Expressions.Expression<Func<SmsConfiguration, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "apiurl" => s => s.ApiUrl,
            "mode" => s => s.Mode,
            "tags" => s => s.Tags,
            "isactive" => s => s.IsActive,
            _ => s => s.Id
        };
    }

    private async Task<(List<SmsConfiguration> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
    {
        var query = context.SmsConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.ApiUrl.Contains(search) ||
                s.Mode.Contains(search) ||
                s.Tags.Contains(search)
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

    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "ApiUrl", string sortDir = "asc")
    {
        var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("API URL,Mode,Tags,Active");

        foreach (var s in items)
        {
            sb.AppendLine($"{EscapeCsv(s.ApiUrl)}," +
                          $"{EscapeCsv(s.Mode)}," +
                          $"{EscapeCsv(s.Tags)}," +
                          $"{(s.IsActive ? "Yes" : "No")}");
        }

        var fileName = $"SMSConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "ApiUrl", string sortDir = "asc")
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
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "ApiUrl", string sortDir = "asc")
    {
        var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("SMS Configurations");

        var headers = new[] { "API URL", "Mode", "Tags", "Active" };
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
            worksheet.Cell(row, 2).Value = s.Mode;
            worksheet.Cell(row, 3).Value = s.Tags;
            worksheet.Cell(row, 4).Value = s.IsActive ? "Yes" : "No";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"SMSConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var smsConfig = await context.SmsConfigurations.FirstOrDefaultAsync(m => m.Id == id);
        if (smsConfig == null) return NotFound();

        return View(smsConfig);
    }

    [RequirePermission("sms.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("sms.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ApiUrl,ApiKey,Mode,Tags,IsActive")] SmsConfiguration smsConfiguration)
    {
        if (ModelState.IsValid)
        {
            context.Add(smsConfiguration);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(smsConfiguration);
    }

    [RequirePermission("sms.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var smsConfiguration = await context.SmsConfigurations.FindAsync(id);
        if (smsConfiguration == null) return NotFound();

        return View(smsConfiguration);
    }

    [RequirePermission("sms.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ApiUrl,ApiKey,Mode,Tags,IsActive")] SmsConfiguration smsConfiguration)
    {
        if (id != smsConfiguration.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                context.Update(smsConfiguration);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SmsConfigurationExists(smsConfiguration.Id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(smsConfiguration);
    }

    [RequirePermission("sms.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var smsConfiguration = await context.SmsConfigurations.FirstOrDefaultAsync(m => m.Id == id);
        if (smsConfiguration == null) return NotFound();

        return View(smsConfiguration);
    }

    [RequirePermission("sms.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var smsConfiguration = await context.SmsConfigurations.FindAsync(id);
        if (smsConfiguration != null)
        {
            context.SmsConfigurations.Remove(smsConfiguration);
        }
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("sms.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            var entity = await context.SmsConfigurations.FindAsync(id);
            if (entity != null)
            {
                context.SmsConfigurations.Remove(entity);
                await context.SaveChangesAsync();
            }
            return Json(new { success = true, message = "SMS configuration deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private bool SmsConfigurationExists(int id)
    {
        return context.SmsConfigurations.Any(e => e.Id == id);
    }
}
