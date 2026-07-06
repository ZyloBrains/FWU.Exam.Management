using FWU.Exam.Management.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

public class TenantSelectController : Controller
{
    private readonly AppDbContext _context;

    public TenantSelectController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var tenants = await _context.Tenants
            .Where(t => t.IsActive)
            .AsNoTracking()
            .ToListAsync();
        return View(tenants);
    }
}
