using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Students;

public class StudentCategory : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(50)]
    public string? StudentCategoryName { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

        public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
