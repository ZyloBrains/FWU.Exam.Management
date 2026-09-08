using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Payments.Controllers;

[Area("Payments")]
[RequirePermission("paymentreconciliation.view")]
public class ESewaVerificationController(IESewaVerificationService verificationService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(string? query, decimal? totalAmount)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            TempData["ErrorMessage"] = "Please enter an eSewa transaction UUID or invoice number.";
            return RedirectToAction(nameof(Index));
        }

        ESewaVerificationResultDto result;

        // Look up by invoice number first — it's an exact, unambiguous local key.
        var trimmed = query.Trim();
        var byInvoice = await verificationService.VerifyByInvoiceAsync(trimmed);
        if (byInvoice.LocalRecord != null || string.Equals(byInvoice.Verdict, "NOREF", StringComparison.OrdinalIgnoreCase))
        {
            result = byInvoice;
        }
        else
        {
            // Otherwise treat the input as an eSewa transaction UUID, using the supplied
            // amount only when no local record exists (eSewa requires the exact amount).
            result = await verificationService.VerifyByUuidAsync(trimmed, totalAmount);
        }

        return View("Index", result);
    }
}