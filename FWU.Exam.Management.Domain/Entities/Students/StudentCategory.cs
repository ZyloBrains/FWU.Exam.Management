using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Students;

public class StudentCategory
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Student Category Name")]
    public string StudentCategoryName { get; set; } = string.Empty;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; } = [];
}
