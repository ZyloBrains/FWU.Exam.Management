using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamScheduleApprovalService
{
    /// <summary>
    /// Creates Pending approval rows for the schedule for every college under the
    /// schedule's faculty that offers the schedule's program. Idempotent.
    /// Called when a schedule is created/edited with a CollegeApprovalDate.
    /// </summary>
    Task CreateApprovalsForScheduleAsync(int examScheduleId);

    /// <summary>All approval rows (pending, approved, rejected) for one college admin's list.</summary>
    Task<List<CollegePendingApprovalDto>> GetApprovalsForCollegeAsync(int collegeId);

    /// <summary>Count of schedules currently pending approval for the college (dashboard badge).</summary>
    Task<int> GetPendingCountForCollegeAsync(int collegeId);

    /// <summary>Per-college status rows shown to the faculty on the schedule Details page.</summary>
    Task<List<ScheduleApprovalStatusDto>> GetApprovalsForScheduleAsync(int examScheduleId);

    /// <summary>College admin approves the requested date for their own college.</summary>
    Task ApproveAsync(int examScheduleId, int collegeId, string approvedByUserId);

    /// <summary>College admin rejects the requested date, optionally proposing an alternate date.</summary>
    Task RejectAsync(int examScheduleId, int collegeId, DateTime? proposedDate, string remarks, string approvedByUserId);

    /// <summary>
    /// Faculty resets every non-approved college back to Pending after editing the
    /// schedule date (used when a college rejected with an alternate date).
    /// </summary>
    Task ResubmitAsync(int examScheduleId);

    /// <summary>Whether the schedule has been approved by the given college.</summary>
    Task<bool> IsScheduleApprovedForCollegeAsync(int examScheduleId, int collegeId);
}
