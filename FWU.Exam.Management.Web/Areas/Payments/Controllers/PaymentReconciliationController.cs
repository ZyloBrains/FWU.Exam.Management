using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Payments.Controllers;

[Area("Payments")]
[RequirePermission("paymentreconciliation.view")]
public class PaymentReconciliationController(IPaymentReconciliationService reconciliationService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? search = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var (items, totalCount) = await reconciliationService.GetPendingPaymentsAsync(search, fromDate, toDate, page, pageSize);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var (items, _) = await reconciliationService.GetPendingPaymentsAsync(null, null, null, 1, 1000);
        var item = items.FirstOrDefault(i => i.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reconcile(int id)
    {
        var result = await reconciliationService.ReconcilePaymentAsync(id);
        if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = result.Success, message = result.Message });

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    // Returns the list of payments that a "Check All Pending" run will process,
    // so the UI can show a pre-flight confirmation before actually running.
    public async Task<IActionResult> PendingPreview()
    {
        var items = await reconciliationService.GetReconcileablePendingAsync();
        return Json(items);
    }

    // Bulk on-demand verification: checks every pending payment against its
    // gateway immediately, so admins can trigger reconciliation instead of
    // relying on (and loading the server with) frequent background polling.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReconcilePending()
    {
        var result = await reconciliationService.ReconcilePendingWithDetailsAsync();

        if (result.AlreadyRunning)
        {
            var runningMessage = "A reconciliation run is already in progress. Please wait for it to finish before starting another.";
            if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, alreadyRunning = true, message = runningMessage, result });
            TempData["ErrorMessage"] = runningMessage;
            return RedirectToAction(nameof(Index));
        }

        var message = result.Confirmed > 0
            ? $"Reconciliation completed. {result.Confirmed} payment(s) were confirmed."
            : "Reconciliation completed. No pending payments could be confirmed right now.";

        if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = true, message, result });

        TempData["SuccessMessage"] = message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkFailed(int id, string? reason)
    {
        var result = await reconciliationService.MarkPaymentFailedAsync(id, reason ?? "Marked as failed by admin.");
        if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = result.Success, message = result.Message });

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPaid(int id, string? remark)
    {
        var result = await reconciliationService.ConfirmPaymentManuallyAsync(id, remark);
        if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = result.Success, message = result.Message });

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}