using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.ViewComponents;

public class OrganizationHeaderViewComponent(AppDbContext context, UserManager<AppUser> userManager) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // 1. Org driven by the current URL route (e.g. /org/{officeCode}/...)
        if (HttpContext.Request.RouteValues.TryGetValue("officeCode", out var officeCodeValue)
            && officeCodeValue is string officeCode
            && !string.IsNullOrEmpty(officeCode))
        {
            var orgByCode = await context.Organizations.FirstOrDefaultAsync(o => o.OfficeCode == officeCode);
            if (orgByCode != null)
                return View(orgByCode);
        }

        // 2. Fall back to the logged-in user's organization
        if (HttpContext.User?.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (user?.OrganizationId != null)
            {
                var orgByUser = await context.Organizations.FindAsync(user.OrganizationId);
                if (orgByUser != null)
                    return View(orgByUser);
            }
        }

        return View((Organization?)null);
    }
}
