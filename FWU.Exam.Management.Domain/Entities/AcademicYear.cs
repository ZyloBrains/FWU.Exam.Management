using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class AcademicYear : ITenantScoped
{
    [DisplayName("Academic Year ID")]
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [DisplayName("Academic Year Code")]
    [Required]
    public int AcademicYearCode { get; set; }

    [MaxLength(50)]
    public string? AcademicYearCodeNepali { get; set; }

    [Required, MaxLength(50)]
    public string? AcademicYearName { get; set; }

    [Required, MaxLength(50)]
    public string? AcademicYearNameNepali { get; set; }

    [MaxLength(50)]
    public string? Remark { get; set; }

    public bool IsRunning { get; set; }
    public bool IsActive { get; set; }
        public virtual ICollection<Batch>? Batches { get; set; }
        public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; }
        public virtual ICollection<ExamSchedule>? ExamSchedules { get; set; }
        public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
