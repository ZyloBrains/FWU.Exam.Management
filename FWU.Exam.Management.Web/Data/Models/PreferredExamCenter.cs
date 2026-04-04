using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Exams;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class PreferredExamCenter
{
    public int Id { get; set; }

    [Required, MaxLength(1024)]
    public string? Name { get; set; }

    public int? CollegeId { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }

    public virtual College? College { get; set; }
    public virtual ICollection<ExamRegistrationCenterChange>? ExamRegistrationCenterChanges { get; set; }
}
