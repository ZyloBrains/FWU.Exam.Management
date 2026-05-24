using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class QuestionSet : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    [Required, MaxLength(255)]
    public string? QuestionSetName { get; set; }
    [MaxLength(1024)]
    public string? Description { get; set; }
    public bool IsActive { get; set; }

        public virtual ICollection<College>? Colleges { get; set; }
}
