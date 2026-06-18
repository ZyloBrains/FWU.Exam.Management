using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Payments.Controllers;

[Area("Payments")]
[RequirePermission("paymenttypes.view")]
public class PaymentTypesController(IPaymentTypeService paymentTypeService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "PaymentTypeName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await paymentTypeService.GetPaymentTypesAsync(page, pageSize, search, sort, sortDir);

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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "PaymentTypeName", string sortDir = "asc")
    {
        var items = await paymentTypeService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Payment Type Name,Logo URL,Status");

        foreach (var pt in items)
        {
            sb.AppendLine($"{EscapeCsv(pt.PaymentTypeName)}," +
                           $"{EscapeCsv(pt.LogoUrl)}," +
                           $"{(pt.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"PaymentTypes_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "PaymentTypeName", string sortDir = "asc")
    {
        var (items, totalCount) = await paymentTypeService.GetPaymentTypesAsync(page, pageSize, search, sort, sortDir);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "PaymentTypeName", string sortDir = "asc")
    {
        var items = await paymentTypeService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("PaymentTypes");

        var headers = new[] { "Payment Type Name", "Logo URL", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.Gray;
        }

        int row = 2;
        foreach (var pt in items)
        {
            worksheet.Cell(row, 1).Value = pt.PaymentTypeName ?? "";
            worksheet.Cell(row, 2).Value = pt.LogoUrl ?? "";
            worksheet.Cell(row, 3).Value = pt.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"PaymentTypes_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var paymentType = await paymentTypeService.GetPaymentTypeByIdAsync(id.Value);
        if (paymentType == null) return NotFound();

        return View(paymentType);
    }

    [RequirePermission("paymenttypes.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("paymenttypes.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,PaymentTypeName,LogoUrl,IsActive")] PaymentType paymentType)
    {
        if (ModelState.IsValid)
        {
            await paymentTypeService.CreatePaymentTypeAsync(paymentType);
            return RedirectToAction(nameof(Index));
        }
        return View(paymentType);
    }

    [RequirePermission("paymenttypes.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var paymentType = await paymentTypeService.GetPaymentTypeByIdAsync(id.Value);
        if (paymentType == null) return NotFound();

        return View(paymentType);
    }

    [RequirePermission("paymenttypes.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,PaymentTypeName,LogoUrl,IsActive")] PaymentType paymentType)
    {
        if (id != paymentType.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await paymentTypeService.UpdatePaymentTypeAsync(paymentType);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await paymentTypeService.PaymentTypeExistsAsync(paymentType.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(paymentType);
    }

    [RequirePermission("paymenttypes.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var paymentType = await paymentTypeService.GetPaymentTypeByIdAsync(id.Value);
        if (paymentType == null) return NotFound();

        return View(paymentType);
    }

    [RequirePermission("paymenttypes.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await paymentTypeService.DeletePaymentTypeAsync(id);
        return RedirectToAction(nameof(Index));
    }
        [RequirePermission("PLACEHOLDER_PERMISSION")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await paymentTypeService.DeletePaymentTypeAsync(id); return Json(new { success = true, message = "Payment type deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
