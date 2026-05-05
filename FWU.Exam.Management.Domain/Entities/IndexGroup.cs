using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class IndexGroup
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string? IndexGroupName { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

        public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
