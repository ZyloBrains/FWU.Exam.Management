using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class QuestionSet : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    [Required, MaxLength(255)]
    [Display(Name = "Question Set Name")]
    public string QuestionSetName { get; set; } = string.Empty;
    [MaxLength(1024)]
    [Display(Name = "Description")]
    public string? Description { get; set; }
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

        public virtual ICollection<College> Colleges { get; set; } = [];
}
