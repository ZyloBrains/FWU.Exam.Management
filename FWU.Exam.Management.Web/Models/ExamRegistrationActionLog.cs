using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ExamRegistrationActionLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ExamRegistrationId { get; set; }
        public DateTime Timestamp { get; set; }

        [Required, MaxLength(255)]
        public string Action { get; set; }

        public string Remarks { get; set; }

        [ForeignKey(nameof(ExamRegistrationId))]
        public virtual ExamRegistration ExamRegistration { get; set; }
    }
}
