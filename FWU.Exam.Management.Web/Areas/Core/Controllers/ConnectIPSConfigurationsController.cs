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
[RequirePermission(Permissions.ConnectIPSView)]
public class ConnectIPSConfigurationsController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "MerchantId", string sortDir = "asc", int pageSize = 10)
    {
        var query = context.ConnectIpsPaymentConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.MerchantId.Contains(search) ||
                s.AppName.Contains(search) ||
                s.GatewayUrl.Contains(search) ||
                s.AppId.Contains(search)
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

    private static System.Linq.Expressions.Expression<Func<ConnectIpsPaymentConfiguration, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "merchantid" => s => s.MerchantId,
            "appname" => s => s.AppName,
            "appid" => s => s.AppId,
            "gatewayurl" => s => s.GatewayUrl,
            "validationapiurl" => s => s.ValidationApiUrl,
            _ => s => s.MerchantId
        };
    }

    private async Task<(List<ConnectIpsPaymentConfiguration> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
    {
        var query = context.ConnectIpsPaymentConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.MerchantId.Contains(search) ||
                s.AppName.Contains(search) ||
                s.GatewayUrl.Contains(search) ||
                s.AppId.Contains(search)
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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "MerchantId", string sortDir = "asc")
    {
        var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Merchant Id,App Id,App Name,Gateway Url,Validation Api Url,Transaction Currency");

        foreach (var s in items)
        {
            sb.AppendLine($"{EscapeCsv(s.MerchantId)}," +
                          $"{EscapeCsv(s.AppId)}," +
                          $"{EscapeCsv(s.AppName)}," +
                          $"{EscapeCsv(s.GatewayUrl)}," +
                          $"{EscapeCsv(s.ValidationApiUrl)}," +
                          $"{EscapeCsv(s.TransactionCurrency)}");
        }

        var fileName = $"ConnectIPSConfigurations_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "MerchantId", string sortDir = "asc")
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

        var configuration = await context.ConnectIpsPaymentConfigurations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (configuration == null) return NotFound();

        return View(configuration);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,GatewayUrl,MerchantId,AppId,AppName,ValidationApiUrl,UsernameForValidationApi,PasswordForValidationApi,PasswordForCreditorPfx,TransactionCurrency")] ConnectIpsPaymentConfiguration configuration)
    {
        if (ModelState.IsValid)
        {
            context.Add(configuration);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(configuration);
    }

    [RequirePermission(Permissions.ConnectIPSEdit)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var configuration = await context.ConnectIpsPaymentConfigurations.FindAsync(id);
        if (configuration == null) return NotFound();
        return View(configuration);
    }

    [RequirePermission(Permissions.ConnectIPSEdit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,GatewayUrl,MerchantId,AppId,AppName,ValidationApiUrl,UsernameForValidationApi,PasswordForValidationApi,PasswordForCreditorPfx,TransactionCurrency")] ConnectIpsPaymentConfiguration configuration)
    {
        if (id != configuration.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                context.Update(configuration);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConnectIPSConfigurationExists(configuration.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(configuration);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var configuration = await context.ConnectIpsPaymentConfigurations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (configuration == null) return NotFound();

        return View(configuration);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var configuration = await context.ConnectIpsPaymentConfigurations.FindAsync(id);
        if (configuration != null)
        {
            context.ConnectIpsPaymentConfigurations.Remove(configuration);
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ConnectIPSConfigurationExists(int id)
    {
        return context.ConnectIpsPaymentConfigurations.Any(e => e.Id == id);
    }
}
