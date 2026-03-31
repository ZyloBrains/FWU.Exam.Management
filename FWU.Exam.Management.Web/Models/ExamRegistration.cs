using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamRegistration:AuditBase
    {
        [Key]
        public int ExamRegistrationId { get; set; }

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

        [ForeignKey(nameof(StudentProgramYearPartId))]
        [ValidateNever]
        public virtual StudentProgramYearPart StudentProgramYearPart { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        [ValidateNever]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(ExamCenterId))]
        [ValidateNever]
        public virtual ExamCenter ExamCenter { get; set; }

        [ForeignKey(nameof(CollegeId))]
        [ValidateNever]
        public virtual College College { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        [ValidateNever]
        public virtual ExamSchedule ExamSchedule { get; set; }

        [ForeignKey(nameof(ProgramsId))]
        [ValidateNever]
        public virtual Programs Program { get; set; }

        [ForeignKey(nameof(ApplicationVoucherId))]
        [ValidateNever]
        public virtual ApplicationVoucher ApplicationVoucher { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamSubjectRegistration> ExamSubjectRegistrations { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamRegistrationActionLog> ExamRegistrationActionLogs { get; set; }
    }
}
