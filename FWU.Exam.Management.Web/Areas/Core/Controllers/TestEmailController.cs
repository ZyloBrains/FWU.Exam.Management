using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
public class TestEmailController(IEmailService emailService, IAuditLogWriter auditLogWriter) : Controller
{
    public IActionResult Index()
    {
        return View(new TestEmailViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(TestEmailViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await emailService.SendEmailAsync(model.ToEmail, model.Subject, model.Body);
            await auditLogWriter.LogAsync(ActivityTypes.TestEmailSent, $"Test email sent to {model.ToEmail}", new { to = model.ToEmail, subject = model.Subject });
            TempData["Success"] = "Email sent successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to send email: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
