using fwu_examination_management_system.Data.Auditing;
using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Subjects;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Students;

public class StudentAdmission: IAuditable
{
    public int Id { get; set; }

    public int BatchId { get; set; }
    public int StudentRegistrationId { get; set; }
    public int ProgramsId { get; set; }
    public int CollegeId { get; set; }
    public int? SectionId { get; set; }
    public DateTime AdmissionDate { get; set; }
    public int? CheckedBy { get; set; }
    public bool IsCompleted { get; set; }

    [MaxLength(50)]
    public string? Cgpa { get; set; }
    public bool IsActive { get; set; }

    [MaxLength(50)]
    public string? CollegeRollNumber { get; set; }

    public int? RepeatBatchId { get; set; }
    public int? SubjectGroupId { get; set; }
    public bool HasFeeExemption { get; set; }

    [ForeignKey(nameof(BatchId))]
    [ValidateNever]
    public virtual Batch Batch { get; set; }

    [ForeignKey(nameof(StudentRegistrationId))]
    [ValidateNever]
    public virtual StudentRegistration StudentRegistration { get; set; }

    [ForeignKey(nameof(ProgramsId))]
    [ValidateNever]
    public virtual Programs Program { get; set; }

    [ForeignKey(nameof(CollegeId))]
    [ValidateNever]
    public virtual College College { get; set; }

    [ForeignKey(nameof(SectionId))]
    [ValidateNever]
    public virtual Section Section { get; set; }

    [ForeignKey(nameof(SubjectGroupId))]
    [ValidateNever]
    public virtual SubjectGroup SubjectGroup { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentProgramYearPart> StudentProgramYearParts { get; set; }
}
