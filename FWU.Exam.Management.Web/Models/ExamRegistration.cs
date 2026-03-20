using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamRegistration
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamRegistrationId { get; set; }

        public int StudentProgramYearPartId { get; set; }
        public int AcademicYearId { get; set; }
        public int? ExamCenterId { get; set; }
        public int CollegeId { get; set; }

        [MaxLength(20)]
        public string ExamRollNumber { get; set; }

        public long? ExamRollNumberCoding { get; set; }
        public decimal? FeeEnclosed { get; set; }
        public decimal? AttendancePercentage { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool? IsVerifiedByCollege { get; set; }
        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public bool? IsWithheld { get; set; }

        [MaxLength(50)]
        public string Sgpa { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool? IsExamRegistered { get; set; }
        public int? TypeId { get; set; }
        public int ExamScheduleId { get; set; }
        public int? RollNumberIndex { get; set; }
        public bool? IsAppliedByStudent { get; set; }
        public int? ProgramsId { get; set; }
        public int? ApplicationVoucherId { get; set; }
        public int? AdminVerifiedBy { get; set; }
        public DateTime? AdminVerifiedDate { get; set; }

        [ForeignKey(nameof(StudentProgramYearPartId))]
        public virtual StudentProgramYearPart StudentProgramYearPart { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(ExamCenterId))]
        public virtual ExamCenter ExamCenter { get; set; }

        [ForeignKey(nameof(CollegeId))]
        public virtual College College { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        public virtual ExamSchedule ExamSchedule { get; set; }

        [ForeignKey(nameof(ProgramsId))]
        public virtual Programs Program { get; set; }

        [ForeignKey(nameof(ApplicationVoucherId))]
        public virtual ApplicationVoucher ApplicationVoucher { get; set; }

        public virtual ICollection<ExamSubjectRegistration> ExamSubjectRegistrations { get; set; }
        public virtual ICollection<ExamRegistrationActionLog> ExamRegistrationActionLogs { get; set; }
    }
}
