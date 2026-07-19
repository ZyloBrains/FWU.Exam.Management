using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class ReportFilterViewModel
{
    [Display(Name = "Academic Year")]
    public int? AcademicYearId { get; set; }

    [Display(Name = "Exam Schedule")]
    public int? ExamScheduleId { get; set; }

    [Display(Name = "Program")]
    public int? ProgramId { get; set; }

    [Display(Name = "College")]
    public int? CollegeId { get; set; }

    [Display(Name = "Semester")]
    public int? SemesterId { get; set; }

    [Display(Name = "Exam Type")]
    public int? ExamTypeId { get; set; }

    [Display(Name = "From Date")]
    public DateTime? FromDate { get; set; }

    [Display(Name = "To Date")]
    public DateTime? ToDate { get; set; }

    public string? ReportTitle { get; set; }
}
