using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class IndexGroup
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Index Group Name")]
    public string? IndexGroupName { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

        public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
