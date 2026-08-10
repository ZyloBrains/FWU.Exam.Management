using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("examapproval.view")]
public class CollegeAdminApprovalsController(
    IExamScheduleApprovalService approvalService,
    IUserContext userContext,
    UserManager<AppUser> userManager,
    IAuditLogWriter auditLogWriter) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (!userContext.CollegeId.HasValue)
        {
            TempData["Error"] = "Your account is not linked to a college.";
            return View(new List<CollegePendingApprovalDto>());
        }

        var items = await approvalService.GetApprovalsForCollegeAsync(userContext.CollegeId.Value);
        ViewBag.CollegeId = userContext.CollegeId.Value;
        ViewBag.PendingCount = items.Count(i => i.Status == Domain.Enums.ExamScheduleApprovalStatus.Pending);
        return View(items);
    }

    [HttpPost]
    [RequirePermission("examapproval.approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int examScheduleId)
    {
        if (!userContext.CollegeId.HasValue)
            return Forbid();

        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        try
        {
            await approvalService.ApproveAsync(examScheduleId, userContext.CollegeId.Value, user.Id);
            await auditLogWriter.LogAsync(ActivityTypes.ExamScheduleApproved, $"College approved exam schedule {examScheduleId}", new { scheduleId = examScheduleId, collegeId = userContext.CollegeId.Value, approvalBy = user.Id }, entityName: "ExamSchedule", entityId: examScheduleId.ToString());
            TempData["Success"] = "Exam schedule approved successfully. Students of your college can now see and register for this exam.";
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "No pending approval was found for this schedule.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [RequirePermission("examapproval.reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(RejectApprovalInput input)
    {
        if (!userContext.CollegeId.HasValue)
            return Forbid();

        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await approvalService.RejectAsync(
                input.ExamScheduleId,
                userContext.CollegeId.Value,
                input.ProposedDate,
                input.Remarks,
                user.Id);
            await auditLogWriter.LogAsync(ActivityTypes.ExamScheduleRejected, $"College rejected exam schedule {input.ExamScheduleId}", new { scheduleId = input.ExamScheduleId, collegeId = userContext.CollegeId.Value, proposedDate = input.ProposedDate?.ToString("yyyy-MM-dd"), remarks = input.Remarks, rejectedBy = user.Id }, entityName: "ExamSchedule", entityId: input.ExamScheduleId.ToString());
            TempData["Success"] = "Exam schedule rejected. The faculty has been notified to review and resubmit.";
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "No pending approval was found for this schedule.";
        }

        return RedirectToAction(nameof(Index));
    }
}
