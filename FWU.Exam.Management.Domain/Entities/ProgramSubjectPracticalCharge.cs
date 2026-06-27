using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class ProgramSubjectPracticalCharge : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Programs Id")]
    public int ProgramsId { get; set; }
    [Range(0, double.MaxValue)]
    [Display(Name = "Practical Subject Charge")]
    public decimal PracticalSubjectCharge { get; set; }

    public virtual Program? Program { get; set; }
}
