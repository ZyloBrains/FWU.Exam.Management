using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamRollNumberSetupDetail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamRollNumberSetupDetailId { get; set; }

        public int ExamRollNumberSetupId { get; set; }
        public int ExamScheduleId { get; set; }
        public int ProgramId { get; set; }
        public int ExamTypeId { get; set; }
        public int CollegeId { get; set; }
        public int StartRollNumber { get; set; }
        public int EndRollNumber { get; set; }
        public int Count { get; set; }

        [MaxLength(50)]
        public string Prefix { get; set; }

        [MaxLength(50)]
        public string Suffix { get; set; }

        [ForeignKey(nameof(ExamRollNumberSetupId))]
        public virtual ExamRollNumberSetup ExamRollNumberSetup { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        public virtual ExamSchedule ExamSchedule { get; set; }

        [ForeignKey(nameof(ProgramId))]
        public virtual Programs Program { get; set; }

        [ForeignKey(nameof(ExamTypeId))]
        public virtual ExamType ExamType { get; set; }

        [ForeignKey(nameof(CollegeId))]
        public virtual College College { get; set; }
    }
}
