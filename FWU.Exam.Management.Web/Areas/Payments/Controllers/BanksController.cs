using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Payments.Controllers;

[Area("Payments")]
public class BanksController(IBankService bankService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "BankName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await bankService.GetBanksAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "BankName", string sortDir = "asc")
    {
        var items = await bankService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Bank Name,Bank Code,Remarks,Status");

        foreach (var b in items)
        {
            sb.AppendLine($"{EscapeCsv(b.BankName)}," +
                           $"{EscapeCsv(b.BankCode ?? "-")}," +
                           $"{EscapeCsv(b.Remarks ?? "-")}," +
                           $"{(b.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Banks_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "BankName", string sortDir = "asc")
    {
        var (items, totalCount) = await bankService.GetBanksAsync(page, pageSize, search, sort, sortDir);

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

        var bank = await bankService.GetBankByIdAsync(id.Value);
        if (bank == null) return NotFound();

        return View(bank);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,BankName,BankCode,Remarks,IsActive")] Bank bank)
    {
        if (ModelState.IsValid)
        {
            await bankService.CreateBankAsync(bank);
            return RedirectToAction(nameof(Index));
        }
        return View(bank);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var bank = await bankService.GetBankByIdAsync(id.Value);
        if (bank == null) return NotFound();

        return View(bank);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,BankName,BankCode,Remarks,IsActive")] Bank bank)
    {
        if (id != bank.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await bankService.UpdateBankAsync(bank);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await bankService.BankExistsAsync(bank.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(bank);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var bank = await bankService.GetBankByIdAsync(id.Value);
        if (bank == null) return NotFound();

        return View(bank);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await bankService.DeleteBankAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
