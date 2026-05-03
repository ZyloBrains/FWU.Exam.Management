using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

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
