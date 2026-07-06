using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamCenterCollege : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public int ExamCenterId { get; set; }
    public virtual ExamCenter? ExamCenter { get; set; }

    public int CollegeId { get; set; }
    public virtual College? College { get; set; }
}
