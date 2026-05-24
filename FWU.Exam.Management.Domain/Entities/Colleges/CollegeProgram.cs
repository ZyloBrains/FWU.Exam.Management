using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Colleges;

public class CollegeProgram : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public DateTime? AffiliationDate { get; set; }
    public int NumberOfStudents { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }

    public int CollegeId { get; set; }
    public virtual College? College { get; set; }

    public int ProgramId { get; set; }
    public virtual Program? Program { get; set; }
}
