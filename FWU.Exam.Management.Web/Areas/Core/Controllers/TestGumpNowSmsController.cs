using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
public class TestGumpNowSmsController(ISmsService smsService, IAuditLogWriter auditLogWriter) : Controller
{
    public IActionResult Index()
    {
        return View(new TestGumpNowSmsViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(TestGumpNowSmsViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await smsService.SendSmsAsync(model.PhoneNumber, model.Message);
            await auditLogWriter.LogAsync(ActivityTypes.TestSmsSent, $"Test GumpNow SMS sent to {model.PhoneNumber}", new { to = model.PhoneNumber, messageLength = model.Message.Length });
            TempData["Success"] = "GumpNow SMS sent successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to send GumpNow SMS: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
