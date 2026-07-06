using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamCenterSymbolRange : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Schedule")]
    public int ExamScheduleId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Center")]
    public int ExamCenterId { get; set; }

    [Display(Name = "From Symbol Number")]
    public long FromSymbolNumber { get; set; }

    [Display(Name = "To Symbol Number")]
    public long ToSymbolNumber { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }
    public virtual ExamCenter? ExamCenter { get; set; }
}
