using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure.Data.Models;

namespace FWU.Exam.Management.Web.ViewComponents;

public class FacultyHeaderViewComponent(AppDbContext context, UserManager<AppUser> userManager) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // 1. Faculty driven by the current URL route (e.g. /faculty/{officeCode}/...)
        if (HttpContext.Request.RouteValues.TryGetValue("officeCode", out var officeCodeValue)
            && officeCodeValue is string officeCode
            && !string.IsNullOrEmpty(officeCode))
        {
            var facultyByCode = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == officeCode);
            if (facultyByCode != null)
                return View(facultyByCode);
        }

        // 2. Fall back to the logged-in user's faculty
        if (HttpContext.User?.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user?.FacultyId != null)
            {
                var facultyByUser = await context.Faculties.FindAsync(user.FacultyId);
                if (facultyByUser != null)
                    return View(facultyByUser);
            }
        }

        return View((Faculty?)null);
    }
}
