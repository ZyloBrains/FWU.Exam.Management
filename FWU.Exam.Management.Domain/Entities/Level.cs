using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Level
{
    public int Id { get; set; }

    [MaxLength(2)]
    public string? LevelCode { get; set; }

    [Required, MaxLength(50)]
    public string? LevelName { get; set; }

    public int? LevelDisplayOrder { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool? IsRunning { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<ExamSchedule>? ExamSchedules { get; set; }
    public virtual ICollection<Program>? Programs { get; set; }
    public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
