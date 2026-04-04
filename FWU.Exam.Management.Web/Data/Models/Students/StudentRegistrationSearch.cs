using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Students;

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
