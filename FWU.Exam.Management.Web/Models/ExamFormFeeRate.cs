using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamFormFeeRate
    {
        [Key]
        public int ExamFormFeeRateId { get; set; }

        public int ExamScheduleId { get; set; }
        public int ExamFormFeeNameId { get; set; }
        public decimal Amount { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int? CollegeTypeId { get; set; }
        public int? ExamTypeId { get; set; }
        public DateTime? ThroughDate { get; set; }
        public DateTime? ApplicableDate { get; set; }
        public bool IsCollegeFee { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        public virtual ExamSchedule ExamSchedule { get; set; }

        [ForeignKey(nameof(ExamFormFeeNameId))]
        public virtual ExamFormFeeName ExamFormFeeName { get; set; }

        [ForeignKey(nameof(CollegeTypeId))]
        public virtual CollegeType CollegeType { get; set; }

        [ForeignKey(nameof(ExamTypeId))]
        public virtual ExamType ExamType { get; set; }
    }
}
