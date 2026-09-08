using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Payments.Controllers;

[Area("Payments")]
[RequirePermission("paymentreconciliation.view")]
public class PaymentVerificationController(IKhaltiLookupVerificationService verificationService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            TempData["ErrorMessage"] = "Please enter a Khalti pidx, transaction id, or invoice number.";
            return RedirectToAction(nameof(Index));
        }

        KhaltiVerificationResultDto result;

        // Look up by invoice number first — it's an exact, unambiguous local key.
        var trimmed = query.Trim();
        var byInvoice = await verificationService.VerifyByInvoiceAsync(trimmed);
        if (byInvoice.LocalRecord != null || string.Equals(byInvoice.Verdict, "NOREF", StringComparison.OrdinalIgnoreCase))
        {
            result = byInvoice;
        }
        else
        {
            // Otherwise treat the input as a pidx (or gateway transaction id) reference.
            result = await verificationService.VerifyByPidxAsync(trimmed);
        }

        return View("Index", result);
    }
}