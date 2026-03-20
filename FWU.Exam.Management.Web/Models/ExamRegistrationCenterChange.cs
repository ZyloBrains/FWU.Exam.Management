using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamRegistrationCenterChange

    {

        [Key]
        public int ExamRegistrationCenterChangeId { get; set; }
        public int ExamRegistrationId { get; set; }

        public int PreferredExamCenterId { get; set; }
        public DateTime RequestedTimestamp { get; set; }
        public int? CurrentExamCenterId { get; set; }

        [ForeignKey(nameof(ExamRegistrationId))]
        public virtual ExamRegistration ExamRegistration { get; set; }

        [ForeignKey(nameof(PreferredExamCenterId))]
        public virtual PreferredExamCenter PreferredExamCenter { get; set; }
    }
}
