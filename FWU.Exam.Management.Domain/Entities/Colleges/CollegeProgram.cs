using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Colleges;

public class CollegeProgram : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Display(Name = "Affiliation Date")]
    public DateTime? AffiliationDate { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Number Of Students")]
    public int NumberOfStudents { get; set; }

    [MaxLength(1024)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "College")]
    public int CollegeId { get; set; }
    public virtual College? College { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Program")]
    public int ProgramId { get; set; }
    public virtual Program? Program { get; set; }
}
