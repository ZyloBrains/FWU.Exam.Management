using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ActiveExamSchedule
    {
        [Key]
        public int ActiveExamScheduleId { get; set; }

        public int ExamScheduleId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        [MaxLength(1024)]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        [ValidateNever]
        public virtual ExamSchedule? ExamSchedule { get; set; }
    }
}
