using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Extensions;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("esewa.view")]
public class ESewaConfigurationsController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "ProductCode", string sortDir = "asc", int pageSize = 10)
    {
        var query = context.ESewaConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.ProductCode.Contains(search) ||
                s.PostUrl.Contains(search) ||
                s.SuccessUrl.Contains(search) ||
                s.VerifyUrl.Contains(search)
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

    private static System.Linq.Expressions.Expression<Func<ESewaConfiguration, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "productcode" => s => s.ProductCode,
            "posturl" => s => s.PostUrl,
            "successurl" => s => s.SuccessUrl,
            "verifyurl" => s => s.VerifyUrl,
            "servicechargeamount" => s => s.ServiceChargeAmount,
            _ => s => s.ProductCode
        };
    }

    private async Task<(List<ESewaConfiguration> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.ESewaConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.ProductCode.Contains(search) ||
                s.PostUrl.Contains(search) ||
                s.SuccessUrl.Contains(search) ||
                s.VerifyUrl.Contains(search)
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


    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "ProductCode", string sortDir = "asc")
    {
        var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Product Code,Post Url,Success Url,Verify Url,Service Charge Amount");

        foreach (var s in items)
        {
            sb.AppendLine($"{s.ProductCode.EscapeCsv()}," +
                          $"{s.PostUrl.EscapeCsv()}," +
                          $"{s.SuccessUrl.EscapeCsv()}," +
                          $"{s.VerifyUrl.EscapeCsv()}," +
                          $"{s.ServiceChargeAmount}");
        }

        var fileName = $"ESewaConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "ProductCode", string sortDir = "asc")
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
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "ProductCode", string sortDir = "asc")
    {
        var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ESewa Configurations");

        var headers = new[] { "Product Code", "Post Url", "Success Url", "Verify Url", "Service Charge Amount" };
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
            worksheet.Cell(row, 1).Value = s.ProductCode;
            worksheet.Cell(row, 2).Value = s.PostUrl;
            worksheet.Cell(row, 3).Value = s.SuccessUrl;
            worksheet.Cell(row, 4).Value = s.VerifyUrl;
            worksheet.Cell(row, 5).Value = s.ServiceChargeAmount;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"ESewaConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var eSewaConfiguration = await context.ESewaConfigurations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (eSewaConfiguration == null) return NotFound();

        return View(eSewaConfiguration);
    }

    [RequirePermission("esewa.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("esewa.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,PostUrl,ProductCode,SecretKey,SuccessUrl,ServiceChargeAmount,VerifyUrl")] ESewaConfiguration eSewaConfiguration)
    {
        if (ModelState.IsValid)
        {
            context.Add(eSewaConfiguration);
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "eSewa configuration created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(eSewaConfiguration);
    }

    [RequirePermission("esewa.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var eSewaConfiguration = await context.ESewaConfigurations.FindAsync(id);
        if (eSewaConfiguration == null) return NotFound();
        return View(eSewaConfiguration);
    }

    [RequirePermission("esewa.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,PostUrl,ProductCode,SecretKey,SuccessUrl,ServiceChargeAmount,VerifyUrl")] ESewaConfiguration eSewaConfiguration)
    {
        if (id != eSewaConfiguration.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existing = await context.ESewaConfigurations.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                if (existing is null) return NotFound();
                eSewaConfiguration.TenantId = existing.TenantId;
                context.Update(eSewaConfiguration);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ESewaConfigurationExists(eSewaConfiguration.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "eSewa configuration updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(eSewaConfiguration);
    }

    [RequirePermission("esewa.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var eSewaConfiguration = await context.ESewaConfigurations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (eSewaConfiguration == null) return NotFound();

        return View(eSewaConfiguration);
    }

    [RequirePermission("esewa.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var eSewaConfiguration = await context.ESewaConfigurations.FindAsync(id);
            if (eSewaConfiguration != null)
            {
                context.ESewaConfigurations.Remove(eSewaConfiguration);
            }

            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "eSewa configuration deleted successfully!";
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

    private bool ESewaConfigurationExists(int id)
    {
        return context.ESewaConfigurations.Any(e => e.Id == id);
    }
        [RequirePermission("esewa.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { var entity = await context.ESewaConfigurations.FindAsync(id); if (entity != null) { context.ESewaConfigurations.Remove(entity); await context.SaveChangesAsync(); } return Json(new { success = true, message = "eSewa configuration deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
