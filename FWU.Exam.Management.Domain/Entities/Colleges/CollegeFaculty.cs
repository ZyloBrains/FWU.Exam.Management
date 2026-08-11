using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Colleges;

public class CollegeFaculty : ITenantScoped
{
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "College")]
    public int CollegeId { get; set; }
    public virtual College? College { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Faculty")]
    public int FacultyId { get; set; }
    public virtual Faculty? Faculty { get; set; }
}
