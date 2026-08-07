using FWU.Exam.Management.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Application.DTOs;

/// <summary>Row shown to a college admin for a schedule awaiting (or that got) their college's approval.</summary>
public class CollegePendingApprovalDto
{
    public int ApprovalId { get; set; }
    public int ExamScheduleId { get; set; }
    public string ExamScheduleName { get; set; } = string.Empty;
    public string? ExamScheduleCode { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public string ExamTypeName { get; set; } = string.Empty;
    public string AcademicYearName { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? StartDateBs { get; set; }
    public string? EndDateBs { get; set; }

    /// <summary>Snapshot of ExamSchedule.CollegeApprovalDate at creation/resubmission.</summary>
    public DateTime? RequestedApprovalDate { get; set; }

    /// <summary>Current value on the exam schedule (may have been edited by the faculty).</summary>
    public DateTime? CollegeApprovalDate { get; set; }

    public ExamScheduleApprovalStatus Status { get; set; }
    public DateTime? ProposedDate { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>Per-college status row shown to the faculty on the schedule Details page.</summary>
public class ScheduleApprovalStatusDto
{
    public int ApprovalId { get; set; }
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public ExamScheduleApprovalStatus Status { get; set; }
    public DateTime? RequestedApprovalDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? RejectedDate { get; set; }
    public DateTime? ProposedDate { get; set; }
    public string? Remarks { get; set; }
    public string? ApprovedByName { get; set; }
}

/// <summary>Input model for the college reject form (alternate date + remarks).</summary>
public class RejectApprovalInput
{
    [Range(1, int.MaxValue)]
    public int ExamScheduleId { get; set; }

    [Display(Name = "Proposed Alternate Date")]
    public DateTime? ProposedDate { get; set; }

    [Required, MaxLength(500)]
    [Display(Name = "Remarks")]
    public string Remarks { get; set; } = string.Empty;
}
