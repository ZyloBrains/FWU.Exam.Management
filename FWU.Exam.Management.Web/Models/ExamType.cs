using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamType
    {
        [Key]
        public int ExamTypeId { get; set; }

        [Required, MaxLength(50)]
        public string ExamTypeName { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        public bool IsActive { get; set; }

        public int Code { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamFormFeeRate> ExamFormFeeRates { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamScheduleBatch> ExamScheduleBatches { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamScheduleDetail> ExamScheduleDetails { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamSubjectRegistration> ExamSubjectRegistrations { get; set; }
    }
}
