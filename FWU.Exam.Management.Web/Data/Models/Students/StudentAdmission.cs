using fwu_examination_management_system.Data.Auditing;
using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Subjects;
using System.ComponentModel.DataAnnotations;

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
    public bool HasFeeExemption { get; set; }

    public virtual Batch? Batch { get; set; }

    public virtual StudentRegistration? StudentRegistration { get; set; }

    public virtual Program? Program { get; set; }

    public virtual College? College { get; set; }

    public virtual Section? Section { get; set; }

    public virtual ICollection<StudentProgramYearPart>? StudentProgramYearParts { get; set; }
}
