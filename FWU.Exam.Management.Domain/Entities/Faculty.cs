using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Faculty
{
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string? FacultyCode { get; set; }

    [Required, MaxLength(200)]
    public string? FacultyName { get; set; }

    [MaxLength(50)]
    public string? ShortName { get; set; }

    [MaxLength(100)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

        public virtual ICollection<Program>? Programs { get; set; }
        public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
