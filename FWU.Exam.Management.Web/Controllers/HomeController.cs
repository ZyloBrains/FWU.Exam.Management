using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FWU.Exam.Management.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        var tenantCode = HttpContext.Items["TenantCode"] as string;
        if (!string.IsNullOrEmpty(tenantCode))
        {
            return Redirect($"/tenant/{tenantCode}/Identity/Account/Login");
        }

        return Redirect("/TenantSelect/Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Entrance()
    {
        return RedirectToAction("VerifyPayment", "Entrance", new { area = "Exams" });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
