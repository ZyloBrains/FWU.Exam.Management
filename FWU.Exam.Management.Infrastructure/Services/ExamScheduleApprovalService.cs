using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

/// <summary>
/// Implements the per-college approval workflow for faculty-wide exam schedules.
///
/// Default interpretation of ExamSchedule.CollegeApprovalDate: the date the faculty
/// proposes and wants the college to confirm. Approving records the confirmation;
/// rejecting proposes an alternate date.
///
/// ===== ALTERNATIVE SEMANTICS (UNCOMMENT IF NEEDED) =====
///
/// (1) DEADLINE interpretation: CollegeApprovalDate is the deadline by which the
///     college must respond (e.g. an exam held later that month). In that case the
///     college is NOT approving the date itself - it is simply acknowledging it.
///     You would:
///       - Validate in the create/edit form: CollegeApprovalDate must be in the future.
///       - Optionally auto-approve rows whose RequestedApprovalDate has passed:
///             if (approval.Status == Pending
///                 && approval.RequestedApprovalDate.HasValue
///                 && approval.RequestedApprovalDate.Value < DateTime.UtcNow)
///                 approval.Status = Approved; approval.ApprovedDate = DateTime.UtcNow;
///
/// (2) EXAM DATE interpretation: CollegeApprovalDate is actually the exam date that
///     requires the college's consent. Approving then also means the schedule's
///     StartDate is accepted. You would:
///       - Validate: CollegeApprovalDate == schedule.StartDate.
///       - On Approve: copy schedule.CollegeApprovalDate into schedule.StartDate.
///
/// The code below implements the DEFAULT interpretation end-to-end.
/// </summary>
public class ExamScheduleApprovalService(AppDbContext context) : IExamScheduleApprovalService
{
    public async Task CreateApprovalsForScheduleAsync(int examScheduleId)
    {
        var schedule = await context.ExamSchedules
            .AsNoTracking()
            .Include(es => es.Program)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);

        if (schedule?.Program?.FacultyId == null)
            return;

        var facultyId = schedule.Program.FacultyId.Value;

        // Colleges under the schedule's faculty (via the CollegeFaculty M2M).
        var facultyCollegeIds = await context.Faculties
            .AsNoTracking()
            .Where(f => f.Id == facultyId)
            .SelectMany(f => f.Colleges)
            .Select(c => c.Id)
            .Distinct()
            .ToListAsync();

        // Colleges that actually offer the schedule's program.
        var programCollegeIds = await context.CollegePrograms
            .AsNoTracking()
            .Where(cp => cp.ProgramId == schedule.ProgramId)
            .Select(cp => cp.CollegeId)
            .ToListAsync();

        // ===== ALTERNATIVE SEMANTIC (UNCOMMENT IF NEEDED) =====
        // Ask EVERY college under the faculty, regardless of whether it offers
        // this program:
        //     var targetCollegeIds = facultyCollegeIds;
        var targetCollegeIds = programCollegeIds.Intersect(facultyCollegeIds).ToList();

        // Fallback: if no college is mapped to the program, ask all faculty colleges.
        if (targetCollegeIds.Count == 0)
            targetCollegeIds = facultyCollegeIds;

        var existing = await context.ExamScheduleCollegeApprovals
            .AsNoTracking()
            .Where(a => a.ExamScheduleId == examScheduleId && a.IsActive)
            .Select(a => a.CollegeId)
            .ToListAsync();

        foreach (var collegeId in targetCollegeIds)
        {
            if (existing.Contains(collegeId))
                continue;

            context.ExamScheduleCollegeApprovals.Add(new ExamScheduleCollegeApproval
            {
                TenantId = schedule.TenantId,
                ExamScheduleId = examScheduleId,
                CollegeId = collegeId,
                Status = ExamScheduleApprovalStatus.Pending,
                RequestedApprovalDate = schedule.CollegeApprovalDate,
                IsActive = true
            });
        }

        if (targetCollegeIds.Count > 0)
            await context.SaveChangesAsync();
    }

    public async Task<List<CollegePendingApprovalDto>> GetApprovalsForCollegeAsync(int collegeId)
    {
        var approvals = await context.ExamScheduleCollegeApprovals
            .AsNoTracking()
            .Where(a => a.CollegeId == collegeId && a.IsActive)
            .Include(a => a.ExamSchedule)
                .ThenInclude(es => es!.Program)
            .Include(a => a.ExamSchedule)
                .ThenInclude(es => es!.Semester)
            .Include(a => a.ExamSchedule)
                .ThenInclude(es => es!.ExamType)
            .Include(a => a.ExamSchedule)
                .ThenInclude(es => es!.AcademicYear)
            .OrderBy(a => a.Status == ExamScheduleApprovalStatus.Pending ? 0 : 1)
            .ThenByDescending(a => a.Id)
            .ToListAsync();

        return approvals.Select(a => new CollegePendingApprovalDto
        {
            ApprovalId = a.Id,
            ExamScheduleId = a.ExamScheduleId,
            ExamScheduleName = a.ExamSchedule?.ExamScheduleName ?? $"Schedule #{a.ExamScheduleId}",
            ExamScheduleCode = a.ExamSchedule?.ExamScheduleCode,
            ProgramName = a.ExamSchedule?.Program?.ProgramName ?? "",
            SemesterName = a.ExamSchedule?.Semester?.Name ?? "",
            ExamTypeName = a.ExamSchedule?.ExamType?.Name ?? "",
            AcademicYearName = a.ExamSchedule?.AcademicYear?.AcademicYearName ?? "",
            StartDate = a.ExamSchedule?.StartDate,
            EndDate = a.ExamSchedule?.EndDate,
            StartDateBs = a.ExamSchedule?.StartDateBs,
            EndDateBs = a.ExamSchedule?.EndDateBs,
            RequestedApprovalDate = a.RequestedApprovalDate,
            CollegeApprovalDate = a.ExamSchedule?.CollegeApprovalDate,
            Status = a.Status,
            ProposedDate = a.ProposedDate,
            Remarks = a.Remarks
        }).ToList();
    }

    public async Task<int> GetPendingCountForCollegeAsync(int collegeId)
    {
        return await context.ExamScheduleCollegeApprovals
            .CountAsync(a => a.CollegeId == collegeId
                          && a.IsActive
                          && a.Status == ExamScheduleApprovalStatus.Pending);
    }

    public async Task<List<ScheduleApprovalStatusDto>> GetApprovalsForScheduleAsync(int examScheduleId)
    {
        var approvals = await context.ExamScheduleCollegeApprovals
            .AsNoTracking()
            .Include(a => a.College)
            .Where(a => a.ExamScheduleId == examScheduleId && a.IsActive)
            .OrderBy(a => a.College!.Name)
            .ToListAsync();

        var userNames = new Dictionary<string, string>();
        var userIds = approvals
            .Where(a => a.ApprovedByUserId != null)
            .Select(a => a.ApprovedByUserId!)
            .Distinct()
            .ToList();
        if (userIds.Count > 0)
        {
            userNames = await context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.Email ?? u.UserName ?? u.Id);
        }

        return approvals.Select(a => new ScheduleApprovalStatusDto
        {
            ApprovalId = a.Id,
            CollegeId = a.CollegeId,
            CollegeName = a.College?.Name ?? $"College #{a.CollegeId}",
            Status = a.Status,
            RequestedApprovalDate = a.RequestedApprovalDate,
            ApprovedDate = a.ApprovedDate,
            RejectedDate = a.RejectedDate,
            ProposedDate = a.ProposedDate,
            Remarks = a.Remarks,
            ApprovedByName = a.ApprovedByUserId != null && userNames.TryGetValue(a.ApprovedByUserId, out var n) ? n : null
        }).ToList();
    }

    public async Task ApproveAsync(int examScheduleId, int collegeId, string approvedByUserId)
    {
        var approval = await context.ExamScheduleCollegeApprovals
            .FirstOrDefaultAsync(a => a.ExamScheduleId == examScheduleId
                                   && a.CollegeId == collegeId
                                   && a.IsActive)
            ?? throw new KeyNotFoundException("No approval record found for this college and schedule.");

        if (approval.Status == ExamScheduleApprovalStatus.Approved)
            return;

        approval.Status = ExamScheduleApprovalStatus.Approved;
        approval.ApprovedDate = DateTime.UtcNow;
        approval.RejectedDate = null;
        approval.ProposedDate = null;
        approval.Remarks = null;
        approval.ApprovedByUserId = approvedByUserId;

        await context.SaveChangesAsync();
    }

    public async Task RejectAsync(int examScheduleId, int collegeId, DateTime? proposedDate, string remarks, string approvedByUserId)
    {
        var approval = await context.ExamScheduleCollegeApprovals
            .FirstOrDefaultAsync(a => a.ExamScheduleId == examScheduleId
                                   && a.CollegeId == collegeId
                                   && a.IsActive)
            ?? throw new KeyNotFoundException("No approval record found for this college and schedule.");

        approval.Status = ExamScheduleApprovalStatus.Rejected;
        approval.RejectedDate = DateTime.UtcNow;
        approval.ApprovedDate = null;
        approval.ProposedDate = proposedDate;
        approval.Remarks = remarks;
        approval.ApprovedByUserId = approvedByUserId;

        await context.SaveChangesAsync();
    }

    public async Task ResubmitAsync(int examScheduleId)
    {
        var schedule = await context.ExamSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(es => es.Id == examScheduleId)
            ?? throw new KeyNotFoundException("Exam schedule not found.");

        var approvals = await context.ExamScheduleCollegeApprovals
            .Where(a => a.ExamScheduleId == examScheduleId && a.IsActive)
            .ToListAsync();

        var changed = false;
        foreach (var approval in approvals)
        {
            approval.RequestedApprovalDate = schedule.CollegeApprovalDate;

            // Approved colleges stay approved; Pending/Rejected go back to Pending.
            if (approval.Status != ExamScheduleApprovalStatus.Approved)
            {
                approval.Status = ExamScheduleApprovalStatus.Pending;
                approval.ApprovedDate = null;
                approval.RejectedDate = null;
                approval.ProposedDate = null;
                approval.Remarks = null;
                approval.ApprovedByUserId = null;
                changed = true;
            }
        }

        if (changed)
            await context.SaveChangesAsync();
    }

    public async Task<bool> IsScheduleApprovedForCollegeAsync(int examScheduleId, int collegeId)
    {
        return await context.ExamScheduleCollegeApprovals
            .AnyAsync(a => a.ExamScheduleId == examScheduleId
                        && a.CollegeId == collegeId
                        && a.IsActive
                        && a.Status == ExamScheduleApprovalStatus.Approved);
    }
}
