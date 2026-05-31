using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

public class TenantSelectController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var tenants = await context.Set<Tenant>()
            .OrderBy(t => t.Name)
            .Select(t => new { t.Id, t.Name, t.OfficeCode, t.Address })
            .ToListAsync();
        return View(tenants);
    }
}
