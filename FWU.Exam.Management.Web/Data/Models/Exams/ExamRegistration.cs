using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Students;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamRegistration
{
    public int Id { get; set; }

    public int StudentProgramYearPartId { get; set; }
    public int AcademicYearId { get; set; }
    public int? ExamCenterId { get; set; }
    public int CollegeId { get; set; }

    [MaxLength(20)]
    public string? ExamRollNumber { get; set; }

    public long? ExamRollNumberCoding { get; set; }
    public decimal? FeeEnclosed { get; set; }
    public decimal? AttendancePercentage { get; set; }
    public DateTime? RegistrationDate { get; set; }
    public bool? IsVerifiedByCollege { get; set; }
    public int? VerifiedBy { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public bool? IsWithheld { get; set; }

    [MaxLength(50)]
    public string? Sgpa { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }
    public bool? IsExamRegistered { get; set; }
    public int? TypeId { get; set; }
    public int ExamScheduleId { get; set; }
    public int? RollNumberIndex { get; set; }
    public bool? IsAppliedByStudent { get; set; }
    public int? ProgramsId { get; set; }
    public int? ApplicationVoucherId { get; set; }
    public int? AdminVerifiedBy { get; set; }
    public DateTime? AdminVerifiedDate { get; set; }

    public virtual StudentProgramYearPart? StudentProgramYearPart { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual ExamCenter? ExamCenter { get; set; }

    public virtual College? College { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual Program? Program { get; set; }

    public virtual ApplicationVoucher? ApplicationVoucher { get; set; }
    public virtual ICollection<ExamSubjectRegistration>? ExamSubjectRegistrations { get; set; }
    public virtual ICollection<ExamRegistrationActionLog>? ExamRegistrationActionLogs { get; set; }
}
