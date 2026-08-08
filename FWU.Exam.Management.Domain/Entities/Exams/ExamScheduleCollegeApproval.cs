using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

/// <summary>
/// Per-college approval record for an (faculty-wide) exam schedule.
/// One row per (ExamSchedule, College). The college admin belonging to that
/// college approves or rejects the requested date; a student only sees the
/// schedule once their college's row is Approved.
/// </summary>
public class ExamScheduleCollegeApproval : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Schedule")]
    public int ExamScheduleId { get; set; }
    public virtual ExamSchedule? ExamSchedule { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "College")]
    public int CollegeId { get; set; }
    public virtual College? College { get; set; }

    [Display(Name = "Status")]
    public ExamScheduleApprovalStatus Status { get; set; } = ExamScheduleApprovalStatus.Pending;

    /// <summary>
    /// Snapshot of ExamSchedule.CollegeApprovalDate at the time the approval row
    /// was created (or resubmitted), so history is preserved if the faculty later
    /// edits the schedule date.
    /// </summary>
    [Display(Name = "Requested Approval Date")]
    public DateTime? RequestedApprovalDate { get; set; }

    [Display(Name = "Approved Date")]
    public DateTime? ApprovedDate { get; set; }

    [Display(Name = "Rejected Date")]
    public DateTime? RejectedDate { get; set; }

    /// <summary>
    /// Alternate date proposed by the college when rejecting.
    /// </summary>
    [Display(Name = "Proposed Date")]
    public DateTime? ProposedDate { get; set; }

    [MaxLength(500)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    /// <summary>
    /// Id of the AppUser (college admin) who approved or rejected.
    /// </summary>
    [MaxLength(450)]
    [Display(Name = "Approved By")]
    public string? ApprovedByUserId { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
}
