using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Students;
using fwu_examination_management_system.Data.Models.Subjects;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models;

public class Programs
{
    [Key]
    public int ProgramsId { get; set; }

    public int LevelId { get; set; }
    public int FacultyId { get; set; }
    public int? BoardId { get; set; }
    public int ProgramPeriodTypeId { get; set; }

    [Required, MaxLength(50)]
    public string ProgramCode { get; set; }

    [Required, MaxLength(255)]
    public string ProgramName { get; set; }

    [Required, MaxLength(50)]
    public string ShortName { get; set; }

    public int Duration { get; set; }
    public int? GrandTotalMarks { get; set; }
    public bool HasMultipleIntakes { get; set; }

    [MaxLength(50)]
    public string? NumberOfSeats { get; set; }

    public int? ScholarshipSeats { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(10)]
    public string? RollNumberPrefix { get; set; }

    [ForeignKey(nameof(LevelId))]
    [ValidateNever]
    public virtual Level Level { get; set; }

    [ForeignKey(nameof(FacultyId))]
    [ValidateNever]
    public virtual Faculty Faculty { get; set; }

    [ForeignKey(nameof(BoardId))]
    [ValidateNever]
    public virtual Board Board { get; set; }

    [ForeignKey(nameof(ProgramPeriodTypeId))]
    [ValidateNever]
    public virtual ProgramPeriodType ProgramPeriodType { get; set; }
    [ValidateNever]
    public virtual ICollection<CollegeProgram> CollegePrograms { get; set; }
    [ValidateNever]
    public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; }
    [ValidateNever]
    public virtual ICollection<ExamRollNumberSetupDetail> ExamRollNumberSetupDetails { get; set; }
    [ValidateNever]
    public virtual ICollection<ProgramSubjectPracticalCharge> ProgramSubjectPracticalCharges { get; set; }
    [ValidateNever]
    public virtual ICollection<ProgramYearPart> ProgramYearParts { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; }
    [ValidateNever]
    public virtual ICollection<SubjectBatch> SubjectBatches { get; set; }
    [ValidateNever]
    public virtual ICollection<SubjectDetail> SubjectDetails { get; set; }
    [ValidateNever]
    public virtual ICollection<SubjectGroup> SubjectGroups { get; set; }
    [ValidateNever]
    public virtual ICollection<UserProgramMap> UserProgramMaps { get; set; }
}
