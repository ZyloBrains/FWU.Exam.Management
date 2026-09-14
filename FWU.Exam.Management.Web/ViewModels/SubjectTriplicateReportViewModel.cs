using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Web.ViewModels;

public class SubjectTriplicateReportViewModel
{
    public ReportFilterViewModel Filter { get; set; } = new();
    public SubjectTriplicateReportDto? Report { get; set; }

    public bool HasReport => Report != null;
}
