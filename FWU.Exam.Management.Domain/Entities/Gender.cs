using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Gender
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Gender Name")]
    public string? GenderName { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

        public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
