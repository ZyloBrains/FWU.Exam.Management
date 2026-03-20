using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamScheduleBatch
    {
        [Key]
        public int ExamScheduleBatchId { get; set; }

        public int ExamScheduleId { get; set; }
        public int ExamTypeId { get; set; }
        public int BatchId { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        public virtual ExamSchedule ExamSchedule { get; set; }

        [ForeignKey(nameof(ExamTypeId))]
        public virtual ExamType ExamType { get; set; }

        [ForeignKey(nameof(BatchId))]
        public virtual Batch Batch { get; set; }
    }
}
