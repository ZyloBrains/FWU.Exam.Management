using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamSchedule
    {
        [Key]
        public int ExamScheduleId { get; set; }

        public int AcademicYearId { get; set; }
        public int LevelId { get; set; }
        public int YearPartId { get; set; }
        public int ExamTypeId { get; set; }

        [Required, MaxLength(50)]
        public string ExamScheduleName { get; set; }

        public DateTime? StartDateAd { get; set; }
        public DateTime? EndDateAd { get; set; }

        [MaxLength(10)]
        public string? StartDateBs { get; set; }

        [MaxLength(10)]
        public string? EndDateBs { get; set; }

        public DateTime? PublishedDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int? ExamScheduleParentId { get; set; }
        public int? NegativeMarks { get; set; }

        [MaxLength(500)]
        public string? ProgramIds { get; set; }

        [MaxLength(500)]
        public string? RegularBatchIds { get; set; }

        [MaxLength(500)]
        public string? PartialBatchIds { get; set; }

        public DateTime? ExtendedDate { get; set; }
        public decimal? ExtendedDateCharge { get; set; }
        public DateTime? CollegeApprovalDate { get; set; }
        public DateTime? AdmissionCardReleaseDate { get; set; }

        [MaxLength(50)]
        public string? ExamScheduleCode { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        [ValidateNever]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(LevelId))]
        [ValidateNever]
        public virtual Level Level { get; set; }

        [ForeignKey(nameof(YearPartId))]
        [ValidateNever]
        public virtual YearPart YearPart { get; set; }

        [ForeignKey(nameof(ExamTypeId))]
        [ValidateNever]
        public virtual ExamType ExamType { get; set; }

        [ForeignKey(nameof(ExamScheduleParentId))]
        public virtual ExamScheduleParent ExamScheduleParent { get; set; }
        [ValidateNever]
        public virtual ICollection<ActiveExamSchedule> ActiveExamSchedules { get; set; }
        [ValidateNever]
        public virtual ICollection<ApplicationVoucher> ApplicationVouchers { get; set; }
        [ValidateNever]
        public virtual ICollection<BillTitle> BillTitles { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamCenter> ExamCenters { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamFormFeeRate> ExamFormFeeRates { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamScheduleBatch> ExamScheduleBatches { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamScheduleDetail> ExamScheduleDetails { get; set; }
        [ValidateNever]
        public virtual ICollection<PaymentRequestLog> PaymentRequestLogs { get; set; }
    }
}
