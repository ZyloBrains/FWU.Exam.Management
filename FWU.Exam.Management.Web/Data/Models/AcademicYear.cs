using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Students;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class AcademicYear
{
    [Key]
    [DisplayName("Academic Year ID")]
    public int AcademicYearId { get; set; } // primary key auto-incremented

    [DisplayName("Academic Year Code")]
    [Required]
    public int AcademicYearCode { get; set; }

    [MaxLength(50)]
    public string? AcademicYearCodeNepali { get; set; }

    [Required, MaxLength(50)]
    public string AcademicYearName { get; set; }

    [Required, MaxLength(50)]
    public string AcademicYearNameNepali { get; set; }

    [MaxLength(50)]
    public string? Remark { get; set; }

    public bool IsRunning { get; set; }
    public bool IsActive { get; set; }
    [ValidateNever]
    public virtual ICollection<Batch> Batches { get; set; }
    [ValidateNever]
    public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; }
    [ValidateNever]
    public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
}
