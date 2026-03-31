using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Notice:AuditBase
    {
        [Key]
        public int NoticeId { get; set; }

        [Required, MaxLength(1024)]
        public string NoticeTitle { get; set; }

        [Required, MaxLength(1024)]
        public string NoticePreview { get; set; }

        public DateTime? PublishedDate { get; set; }

        [Required]
        public string NoticeContent { get; set; }

    }
}
