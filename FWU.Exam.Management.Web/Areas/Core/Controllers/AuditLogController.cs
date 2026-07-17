using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("auditlog.view")]
public class AuditLogController(IAuditLogService auditLogService) : Controller
{
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 25,
        string? entityName = null,
        string? action = null,
        string? userName = null,
        DateTime? from = null,
        DateTime? to = null,
        string? search = null)
    {
        var (items, totalCount) = await auditLogService.GetAuditLogsAsync(
            page, pageSize, entityName, action, userName, from, to, search);

        ViewBag.TotalCount = totalCount;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.EntityName = entityName;
        ViewBag.Action = action;
        ViewBag.UserName = userName;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        ViewBag.Search = search;

        ViewData["EntityNames"] = await auditLogService.GetEntityNamesAsync();
        ViewData["Actions"] = new List<string> { "Created", "Updated", "Deleted" };

        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var (items, _) = await auditLogService.GetAuditLogsAsync(1, 1000, null, null, null, null, null, null);
        var log = items.FirstOrDefault(a => a.Id == id);
        if (log == null) return NotFound();
        return View(log);
    }
}
