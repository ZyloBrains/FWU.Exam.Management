using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Teachers;

public class TeacherSubjectAssignment : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required]
    public string TeacherUserId { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int SubjectOfferingId { get; set; }

    public int? ExamScheduleId { get; set; }

    public bool IsActive { get; set; }

    public virtual SubjectOffering? SubjectOffering { get; set; }
    public virtual ExamSchedule? ExamSchedule { get; set; }
}
