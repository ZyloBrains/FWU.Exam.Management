using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Permissions;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission(Permissions.PaymentVerificationView)]
public class PaymentVerificationsController(IPaymentVerificationService service) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null,
        DateTime? fromDate = null, DateTime? toDate = null,
        string sort = "VoucherDate", string sortDir = "desc", int pageSize = 10)
    {
        var (items, totalCount) = await service.GetPagedAsync(search, fromDate, toDate, sort, sortDir, page, pageSize);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string? voucherNo)
    {
        if (string.IsNullOrWhiteSpace(voucherNo))
            return RedirectToAction(nameof(Index));

        var voucher = await service.GetByVoucherNumberAsync(voucherNo);
        if (voucher == null)
        {
            TempData["ErrorMessage"] = $"No payment found for voucher / transaction \"{voucherNo.Trim()}\".";
            return RedirectToAction(nameof(Index));
        }

        return View(voucher);
    }

    [HttpGet]
    [RequirePermission(Permissions.PaymentVerificationExport)]
    public async Task<IActionResult> ExportToExcel(string? search = null,
        DateTime? fromDate = null, DateTime? toDate = null, string sort = "VoucherDate", string sortDir = "desc")
    {
        var data = await service.GetAllForExportAsync(search, fromDate, toDate, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Payment Verifications");

        worksheet.Cell(1, 1).Value = "Voucher Number";
        worksheet.Cell(1, 2).Value = "Student Name";
        worksheet.Cell(1, 3).Value = "Amount";
        worksheet.Cell(1, 4).Value = "Transaction Code";
        worksheet.Cell(1, 5).Value = "Payment Gateway";
        worksheet.Cell(1, 6).Value = "Requested Time";
        worksheet.Cell(1, 7).Value = "Voucher Date";
        worksheet.Cell(1, 8).Value = "Contact Number";
        worksheet.Cell(1, 9).Value = "Branch";
        worksheet.Cell(1, 10).Value = "Roll No";
        worksheet.Cell(1, 11).Value = "Exam";
        worksheet.Cell(1, 12).Value = "Academic Year";
        worksheet.Cell(1, 13).Value = "College";
        worksheet.Cell(1, 14).Value = "Program";

        var headerRange = worksheet.Range(1, 1, 1, 14);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach (var item in data)
        {
            worksheet.Cell(row, 1).Value = item.VoucherNumber;
            worksheet.Cell(row, 2).Value = item.StudentName;
            worksheet.Cell(row, 3).Value = item.Amount;
            worksheet.Cell(row, 4).Value = item.TransactionCode ?? "";
            worksheet.Cell(row, 5).Value = item.PaymentGateway ?? "";
            worksheet.Cell(row, 6).Value = item.RequestedTime?.ToString("yyyy-MM-dd hh:mm:ss tt") ?? "";
            worksheet.Cell(row, 7).Value = item.VoucherDate?.ToString("yyyy-MM-dd HH:mm") ?? "";
            worksheet.Cell(row, 8).Value = item.ContactNumber;
            worksheet.Cell(row, 9).Value = item.Branch ?? "";
            worksheet.Cell(row, 10).Value = item.RollNumber ?? "";
            worksheet.Cell(row, 11).Value = item.ExamName ?? "";
            worksheet.Cell(row, 12).Value = item.AcademicYear ?? "";
            worksheet.Cell(row, 13).Value = item.College ?? "";
            worksheet.Cell(row, 14).Value = item.Program ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileBytes = stream.ToArray();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"PaymentVerifications_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpGet]
    [RequirePermission(Permissions.PaymentVerificationExport)]
    public async Task<IActionResult> ExportToCsv(string? search = null,
        DateTime? fromDate = null, DateTime? toDate = null, string sort = "VoucherDate", string sortDir = "desc")
    {
        var data = await service.GetAllForExportAsync(search, fromDate, toDate, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Voucher Number,Student Name,Amount,Transaction Code,Payment Gateway,Requested Time,Voucher Date,Contact Number,Branch,Roll No,Exam,Academic Year,College,Program");
        foreach (var item in data)
        {
            sb.AppendLine($"{item.VoucherNumber.EscapeCsv()},{item.StudentName.EscapeCsv()},{item.Amount},{(item.TransactionCode ?? "").EscapeCsv()},{(item.PaymentGateway ?? "").EscapeCsv()},{item.RequestedTime:yyyy-MM-dd hh:mm:ss tt},{item.VoucherDate:yyyy-MM-dd HH:mm},{item.ContactNumber.EscapeCsv()},{(item.Branch ?? "").EscapeCsv()},{(item.RollNumber ?? "").EscapeCsv()},{(item.ExamName ?? "").EscapeCsv()},{(item.AcademicYear ?? "").EscapeCsv()},{(item.College ?? "").EscapeCsv()},{(item.Program ?? "").EscapeCsv()}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", $"PaymentVerifications_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet]
    [RequirePermission(Permissions.PaymentVerificationExport)]
    public async Task<IActionResult> ExportToPdf(string? search = null,
        DateTime? fromDate = null, DateTime? toDate = null, string sort = "VoucherDate", string sortDir = "desc")
    {
        var data = await service.GetAllForExportAsync(search, fromDate, toDate, sort, sortDir);
        return View("PrintPdf", data);
    }
}
