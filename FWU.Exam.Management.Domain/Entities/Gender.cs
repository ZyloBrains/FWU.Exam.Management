using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Gender
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string? GenderName { get; set; }

    public bool IsActive { get; set; }

        public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
