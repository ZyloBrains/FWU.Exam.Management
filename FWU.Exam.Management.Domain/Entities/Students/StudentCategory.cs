using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Students;

public class StudentCategory
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string? StudentCategoryName { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

        public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
