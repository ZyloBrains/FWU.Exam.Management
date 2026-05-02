using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Students;
using fwu_examination_management_system.Data.Models.Subjects;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class Program
{
    public int Id { get; set; }

    public int LevelId { get; set; }
    public int FacultyId { get; set; }
    public int? BoardId { get; set; }
    public int ProgramPeriodTypeId { get; set; }

    [Required, MaxLength(50)]
    public string? ProgramCode { get; set; }

    [Required, MaxLength(255)]
    public string? ProgramName { get; set; }

    [Required, MaxLength(50)]
    public string? ShortName { get; set; }

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

    public virtual Level? Level { get; set; }

    public virtual Faculty? Faculty { get; set; }

    public virtual Board? Board { get; set; }

    public virtual ProgramPeriodType? ProgramPeriodType { get; set; }
    public virtual ICollection<CollegeProgram>? CollegePrograms { get; set; }
    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; }
    public virtual ICollection<ExamRollNumberSetupDetail>? ExamRollNumberSetupDetails { get; set; }
    public virtual ICollection<ProgramSubjectPracticalCharge>? ProgramSubjectPracticalCharges { get; set; }
    public virtual ICollection<ProgramYearPart>? ProgramYearParts { get; set; }
    public virtual ICollection<StudentAdmission>? StudentAdmissions { get; set; }
    public virtual ICollection<SubjectDetail>? SubjectDetails { get; set; }
    public virtual ICollection<UserProgramMap>? UserProgramMaps { get; set; }
}
