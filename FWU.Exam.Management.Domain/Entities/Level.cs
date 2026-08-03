using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Level
{
    public int Id { get; set; }

    [MaxLength(30)]
    [Display(Name = "Level Code")]
    public string? LevelCode { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Level Name")]
    public string LevelName { get; set; } = string.Empty;

    [Display(Name = "Level Display Order")]
    public int? LevelDisplayOrder { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Running")]
    public bool? IsRunning { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    public virtual ICollection<ExamSchedule> ExamSchedules { get; set; } = [];
    public virtual ICollection<Program> Programs { get; set; } = [];
    public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; } = [];
}

