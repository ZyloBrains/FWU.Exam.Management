using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Web.ViewModels;

public class AttendanceSheetReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public AttendanceSheetReportDto? Report { get; set; }

    public bool HasReport => Report != null;
}