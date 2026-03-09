using fwu_examination_management_system.Data;
using fwu_examination_management_system.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace fwu_examination_management_system.Controllers
{
    public class HomeController(UserManager<AppUser> userManager, ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationId != null)
                {
                    var org = await context.Organizations.FindAsync(user.OrganizationId);
                    if (org != null)
                    {
                        ViewBag.Organization = org;
                        ViewBag.UserCount = await userManager.Users
                            .CountAsync(u => u.OrganizationId == org.Id);
                    }
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
