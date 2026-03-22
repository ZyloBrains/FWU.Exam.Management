using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamAttendanceStatus
    {
        [Key]
        public int ExamAttendanceStatusId { get; set; }

        [Required, MaxLength(50)]
        public string ExamAttendanceStatusName { get; set; }
    }
}
