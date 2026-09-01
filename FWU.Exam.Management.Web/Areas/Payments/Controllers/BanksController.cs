using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Payments.Controllers;

[Area("Payments")]
[RequirePermission("banks.view")]
public class BanksController(IBankService bankService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "BankName", string sortDir = "asc", int pageSize = 10)
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


    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "BankName", string sortDir = "asc")
    {
        var items = await bankService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Bank Name,Bank Code,Remarks,Status");

        foreach (var b in items)
        {
            sb.AppendLine($"{b.BankName.EscapeCsv()}," +
                           $"{(b.BankCode ?? "-").EscapeCsv()}," +
                           $"{(b.Remarks ?? "-").EscapeCsv()}," +
                           $"{(b.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Banks_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "BankName", string sortDir = "asc")
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

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "BankName", string sortDir = "asc")
    {
        var items = await bankService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Banks");

        var headers = new[] { "Bank Name", "Bank Code", "Remarks", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.Gray;
        }

        int row = 2;
        foreach (var b in items)
        {
            worksheet.Cell(row, 1).Value = b.BankName ?? "";
            worksheet.Cell(row, 2).Value = b.BankCode ?? "-";
            worksheet.Cell(row, 3).Value = b.Remarks ?? "-";
            worksheet.Cell(row, 4).Value = b.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"Banks_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [RequirePermission("banks.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("banks.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,BankName,BankCode,Remarks,IsActive")] Bank bank)
    {
        if (ModelState.IsValid)
        {
            await bankService.CreateBankAsync(bank);
            TempData["SuccessMessage"] = "Bank created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(bank);
    }

    [RequirePermission("banks.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var bank = await bankService.GetBankByIdAsync(id.Value);
        if (bank == null) return NotFound();

        return View(bank);
    }

    [RequirePermission("banks.edit")]
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
            TempData["SuccessMessage"] = "Bank updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(bank);
    }

    [RequirePermission("banks.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var bank = await bankService.GetBankByIdAsync(id.Value);
        if (bank == null) return NotFound();

        return View(bank);
    }

    [RequirePermission("banks.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await bankService.DeleteBankAsync(id);
            TempData["SuccessMessage"] = "Bank deleted successfully!";
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
        [RequirePermission("banks.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await bankService.DeleteBankAsync(id); return Json(new { success = true, message = "Bank deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
