using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class AcademicYear
{
    [Display(Name = "Academic Year ID")]
    public int Id { get; set; }

    [Display(Name = "Academic Year Code")]
    [Required, MaxLength(16)]
    public string? AcademicYearCode { get; set; }

    [MaxLength(50)]
    [Display(Name = "Academic Year Code Nepali")]
    public string? AcademicYearCodeNepali { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Academic Year Name")]
    public string? AcademicYearName { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Academic Year Name Nepali")]
    public string? AcademicYearNameNepali { get; set; }

    [MaxLength(50)]
    [Display(Name = "Remark")]
    public string? Remark { get; set; }

    [Display(Name = "Is Running")]
    public bool IsRunning { get; set; }
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
        public virtual ICollection<Batch>? Batches { get; set; }
        public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; }
        public virtual ICollection<ExamSchedule>? ExamSchedules { get; set; }
        public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
