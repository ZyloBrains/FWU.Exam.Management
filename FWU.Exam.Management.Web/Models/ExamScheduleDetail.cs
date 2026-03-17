using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamScheduleDetail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamScheduleDetailId { get; set; }

        public int ExamScheduleId { get; set; }
        public int ExamTypeId { get; set; }
        public int SubjectDetailId { get; set; }
        public DateTime ExamDate { get; set; }

        [MaxLength(10)]
        public string ExamDateBs { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        public virtual ExamSchedule ExamSchedule { get; set; }

        [ForeignKey(nameof(ExamTypeId))]
        public virtual ExamType ExamType { get; set; }

        [ForeignKey(nameof(SubjectDetailId))]
        public virtual SubjectDetail SubjectDetail { get; set; }
    }
}
