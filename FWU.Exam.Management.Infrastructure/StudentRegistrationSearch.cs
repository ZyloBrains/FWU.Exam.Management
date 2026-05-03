using System.ComponentModel.DataAnnotations;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Infrastructure;

public class StudentRegistrationSearch
{
    public int Id { get; set; }

    public string? SearchCriteria { get; set; }
    public DateTime SearchDate { get; set; }
    public string? UserId { get; set; }

    public string? SearchResults { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public virtual AppUser? User { get; set; }
    public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
