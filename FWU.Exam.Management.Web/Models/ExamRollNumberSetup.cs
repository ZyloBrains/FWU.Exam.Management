using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamRollNumberSetup
    {
        [Key]
        public int ExamRollNumberSetupId { get; set; }

        public int ExamScheduleParentId { get; set; }
        public int FirstExamRollNumber { get; set; }

        [MaxLength(50)]
        public string Prefix { get; set; }

        [MaxLength(50)]
        public string Suffix { get; set; }

        public int MinimumRollNumberLength { get; set; }
        public int Round { get; set; }
        public int MinimumGap { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ForeignKey(nameof(ExamScheduleParentId))]
        public virtual ExamScheduleParent ExamScheduleParent { get; set; }

        public virtual ICollection<ExamRollNumberSetupDetail> ExamRollNumberSetupDetails { get; set; }
    }
}
