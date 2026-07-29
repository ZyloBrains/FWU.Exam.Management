using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
public class TestGumpNowSmsController(ISmsService smsService) : Controller
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
            TempData["Success"] = "GumpNow SMS sent successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to send GumpNow SMS: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
