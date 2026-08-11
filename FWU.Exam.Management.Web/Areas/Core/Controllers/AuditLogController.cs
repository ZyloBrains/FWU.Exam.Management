using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
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
        string? actionType = null,
        string? activityType = null,
        string? severity = null,
        string? userName = null,
        DateTime? from = null,
        DateTime? to = null,
        string? search = null)
    {
        string? kind;
        if (Request.Query.TryGetValue("kind", out var kindValue))
            kind = kindValue.ToString();
        else
            kind = AuditLogKinds.Activity;

        var (items, totalCount) = await auditLogService.GetAuditLogsAsync(
            page, pageSize, entityName, actionType, userName, from, to, search, kind, activityType, severity);

        ViewBag.TotalCount = totalCount;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.Kind = kind;
        ViewBag.EntityName = entityName;
        ViewBag.Action = actionType;
        ViewBag.ActivityType = activityType;
        ViewBag.Severity = severity;
        ViewBag.UserName = userName;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        ViewBag.Search = search;

        ViewData["Kinds"] = new List<string> { AuditLogKinds.DataChange, AuditLogKinds.Activity };
        ViewData["Severities"] = new List<string> { AuditSeverity.Info, AuditSeverity.Warning, AuditSeverity.Error };
        ViewData["EntityNames"] = await auditLogService.GetEntityNamesAsync();
        ViewData["ActivityTypes"] = await auditLogService.GetActivityTypesAsync();
        ViewData["Actions"] = new List<string> { "Created", "Updated", "Deleted" };

        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var log = await auditLogService.GetByIdAsync(id);
        if (log == null) return NotFound();
        return View(log);
    }
}
