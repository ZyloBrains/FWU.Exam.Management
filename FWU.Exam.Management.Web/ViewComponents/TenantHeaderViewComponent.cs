using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.ViewComponents;

public class TenantHeaderViewComponent(AppDbContext context, UserManager<AppUser> userManager) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // 1. Tenant driven by the current URL route (e.g. /tenant/{tenantCode}/...)
        var routeData = ViewContext.RouteData;
        var tenantCode = routeData?.Values["tenantCode"]?.ToString();
        if (!string.IsNullOrWhiteSpace(tenantCode))
        {
            var tenantByCode = await context.Tenants.FirstOrDefaultAsync(t => t.OfficeCode == tenantCode);
            if (tenantByCode != null)
                return View(tenantByCode);
        }

        // 2. Fall back to the logged-in user's tenant
        var user = await userManager.GetUserAsync(HttpContext.User);
        if (user?.TenantId != null)
        {
            var tenantByUser = await context.Tenants.FindAsync(user.TenantId);
            if (tenantByUser != null)
                return View(tenantByUser);
        }

        return View((Tenant?)null);
    }
}
