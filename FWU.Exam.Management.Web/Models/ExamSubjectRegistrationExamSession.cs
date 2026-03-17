using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamSubjectRegistrationExamSession
    {
        [Key]
        public int ExamSubjectRegistrationId { get; set; }

        public DateTime ExamStartedDateTime { get; set; }
        public bool IsSubmitted { get; set; }
        public decimal? ObtainedMarks { get; set; }
        public DateTime? ExamSubmittedDateTime { get; set; }
        public bool? IsAutoSubmitted { get; set; }
        public DateTime LastStatusSyncDateTime { get; set; }

        [ForeignKey(nameof(ExamSubjectRegistrationId))]
        public virtual ExamSubjectRegistration ExamSubjectRegistration { get; set; }
    }
}
