using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamScheduleParent
    {
        [Key]
        public int ExamScheduleParentId { get; set; }

        [Required, MaxLength(255)]
        public string ExamScheduleParentName { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<BankVoucher> BankVouchers { get; set; }
        public virtual ICollection<ExamRollNumberSetup> ExamRollNumberSetups { get; set; }
        public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
    }
}
