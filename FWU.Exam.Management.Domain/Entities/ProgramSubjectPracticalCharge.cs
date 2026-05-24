using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Domain.Entities;

public class ProgramSubjectPracticalCharge : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public int ProgramsId { get; set; }
    public decimal PracticalSubjectCharge { get; set; }

    public virtual Program? Program { get; set; }
}
