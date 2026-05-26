using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace FWU.Exam.Management.Web.Areas.Payments.Controllers;

[Area("Payments")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin")]
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
        sb.AppendLine("Payment Type Name,Status");

        foreach (var pt in items)
        {
            sb.AppendLine($"{EscapeCsv(pt.PaymentTypeName)}," +
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

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var paymentType = await paymentTypeService.GetPaymentTypeByIdAsync(id.Value);
        if (paymentType == null) return NotFound();

        return View(paymentType);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,PaymentTypeName,IsActive")] PaymentType paymentType)
    {
        if (ModelState.IsValid)
        {
            await paymentTypeService.CreatePaymentTypeAsync(paymentType);
            return RedirectToAction(nameof(Index));
        }
        return View(paymentType);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var paymentType = await paymentTypeService.GetPaymentTypeByIdAsync(id.Value);
        if (paymentType == null) return NotFound();

        return View(paymentType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,PaymentTypeName,IsActive")] PaymentType paymentType)
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

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var paymentType = await paymentTypeService.GetPaymentTypeByIdAsync(id.Value);
        if (paymentType == null) return NotFound();

        return View(paymentType);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await paymentTypeService.DeletePaymentTypeAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
