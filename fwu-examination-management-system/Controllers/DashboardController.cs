using Microsoft.AspNetCore.Mvc;

namespace fwu_examination_management_system.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
