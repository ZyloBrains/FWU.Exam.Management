using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities.Payments;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using FWU.Exam.Management.Domain.Entities.Permissions;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission(Permissions.KhaltiView)]
public class KhaltiConfigurationsController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "ProductName", string sortDir = "asc", int pageSize = 10)
    {
        var query = context.KhaltiConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.ProductName.Contains(search) ||
                s.PostUrl.Contains(search) ||
                s.ReturnUrl.Contains(search) ||
                s.VerifyUrl.Contains(search) ||
                s.WebsiteUrl.Contains(search)
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

    private static System.Linq.Expressions.Expression<Func<KhaltiConfiguration, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "productname" => s => s.ProductName,
            "posturl" => s => s.PostUrl,
            "returnurl" => s => s.ReturnUrl,
            "verifyurl" => s => s.VerifyUrl,
            "websiteurl" => s => s.WebsiteUrl,
            "servicecharge" => s => s.ServiceCharge,
            _ => s => s.ProductName
        };
    }

    private async Task<(List<KhaltiConfiguration> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.KhaltiConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.ProductName.Contains(search) ||
                s.PostUrl.Contains(search) ||
                s.ReturnUrl.Contains(search) ||
                s.VerifyUrl.Contains(search) ||
                s.WebsiteUrl.Contains(search)
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

    private string EscapeCsv(string? field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "ProductName", string sortDir = "asc")
    {
        var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Product Name,Return Url,Website Url,Post Url,Verify Url,Service Charge");

        foreach (var s in items)
        {
            sb.AppendLine($"{EscapeCsv(s.ProductName)}," +
                          $"{EscapeCsv(s.ReturnUrl)}," +
                          $"{EscapeCsv(s.WebsiteUrl)}," +
                          $"{EscapeCsv(s.PostUrl)}," +
                          $"{EscapeCsv(s.VerifyUrl)}," +
                          $"{s.ServiceCharge}");
        }

        var fileName = $"KhaltiConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "ProductName", string sortDir = "asc")
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
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "ProductName", string sortDir = "asc")
    {
        var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Khalti Configurations");

        var headers = new[] { "Product Name", "Return Url", "Website Url", "Post Url", "Verify Url", "Service Charge" };
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
            worksheet.Cell(row, 1).Value = s.ProductName;
            worksheet.Cell(row, 2).Value = s.ReturnUrl;
            worksheet.Cell(row, 3).Value = s.WebsiteUrl;
            worksheet.Cell(row, 4).Value = s.PostUrl;
            worksheet.Cell(row, 5).Value = s.VerifyUrl;
            worksheet.Cell(row, 6).Value = s.ServiceCharge;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"KhaltiConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var khaltiConfiguration = await context.KhaltiConfigurations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (khaltiConfiguration == null) return NotFound();

        return View(khaltiConfiguration);
    }

    [RequirePermission(Permissions.KhaltiCreate)]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission(Permissions.KhaltiCreate)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ReturnUrl,WebsiteUrl,Amount,ProductName,AuthorizationKey,ServiceCharge,PostUrl,VerifyUrl")] KhaltiConfiguration khaltiConfiguration)
    {
        if (ModelState.IsValid)
        {
            context.Add(khaltiConfiguration);
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Khalti configuration created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(khaltiConfiguration);
    }

    [RequirePermission(Permissions.KhaltiEdit)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var khaltiConfiguration = await context.KhaltiConfigurations.FindAsync(id);
        if (khaltiConfiguration == null) return NotFound();
        return View(khaltiConfiguration);
    }

    [HttpPost]
    [RequirePermission(Permissions.KhaltiEdit)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ReturnUrl,WebsiteUrl,Amount,ProductName,AuthorizationKey,ServiceCharge,PostUrl,VerifyUrl")] KhaltiConfiguration khaltiConfiguration)
    {
        if (id != khaltiConfiguration.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existing = await context.KhaltiConfigurations.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                if (existing is null) return NotFound();
                khaltiConfiguration.TenantId = existing.TenantId;
                context.Update(khaltiConfiguration);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KhaltiConfigurationExists(khaltiConfiguration.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Khalti configuration updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(khaltiConfiguration);
    }

    [RequirePermission(Permissions.KhaltiDelete)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var khaltiConfiguration = await context.KhaltiConfigurations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (khaltiConfiguration == null) return NotFound();

        return View(khaltiConfiguration);
    }

    [RequirePermission(Permissions.KhaltiDelete)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var khaltiConfiguration = await context.KhaltiConfigurations.FindAsync(id);
            if (khaltiConfiguration != null)
            {
                context.KhaltiConfigurations.Remove(khaltiConfiguration);
            }

            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Khalti configuration deleted successfully!";
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

    private bool KhaltiConfigurationExists(int id)
    {
        return context.KhaltiConfigurations.Any(e => e.Id == id);
    }
        [RequirePermission(Permissions.KhaltiDelete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { var entity = await context.KhaltiConfigurations.FindAsync(id); if (entity != null) { context.KhaltiConfigurations.Remove(entity); await context.SaveChangesAsync(); } return Json(new { success = true, message = "Khalti configuration deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
