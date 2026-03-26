using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamCenter
    {
        [Key]
        public int ExamCenterId { get; set; }

        public int ExamScheduleId { get; set; }
        public int CollegeId { get; set; }

        [MaxLength(255)]
        public string? Remark { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int Code { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        [ValidateNever]
        public virtual ExamSchedule ExamSchedule { get; set; }

        [ForeignKey(nameof(CollegeId))]
        [ValidateNever]
        public virtual College College { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamCenterDetail> ExamCenterDetails { get; set; }
        [ValidateNever]
        public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; }
    }
}
