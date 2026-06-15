using System.Text;
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
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "ProductName", string sortDir = "asc", int pageSize = 10)
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

    private async Task<(List<KhaltiConfiguration> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
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

    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "ProductName", string sortDir = "asc")
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

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "ProductName", string sortDir = "asc")
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

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var khaltiConfiguration = await context.KhaltiConfigurations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (khaltiConfiguration == null) return NotFound();

        return View(khaltiConfiguration);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ReturnUrl,WebsiteUrl,Amount,ProductName,AuthorizationKey,ServiceCharge,PostUrl,VerifyUrl")] KhaltiConfiguration khaltiConfiguration)
    {
        if (ModelState.IsValid)
        {
            context.Add(khaltiConfiguration);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(khaltiConfiguration);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var khaltiConfiguration = await context.KhaltiConfigurations.FindAsync(id);
        if (khaltiConfiguration == null) return NotFound();
        return View(khaltiConfiguration);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ReturnUrl,WebsiteUrl,Amount,ProductName,AuthorizationKey,ServiceCharge,PostUrl,VerifyUrl")] KhaltiConfiguration khaltiConfiguration)
    {
        if (id != khaltiConfiguration.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                context.Update(khaltiConfiguration);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KhaltiConfigurationExists(khaltiConfiguration.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(khaltiConfiguration);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var khaltiConfiguration = await context.KhaltiConfigurations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (khaltiConfiguration == null) return NotFound();

        return View(khaltiConfiguration);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var khaltiConfiguration = await context.KhaltiConfigurations.FindAsync(id);
        if (khaltiConfiguration != null)
        {
            context.KhaltiConfigurations.Remove(khaltiConfiguration);
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool KhaltiConfigurationExists(int id)
    {
        return context.KhaltiConfigurations.Any(e => e.Id == id);
    }
}
