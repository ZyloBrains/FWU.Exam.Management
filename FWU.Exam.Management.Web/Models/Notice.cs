using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class Notice
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NoticeId { get; set; }

        [Required, MaxLength(1024)]
        public string NoticeTitle { get; set; }

        [Required, MaxLength(1024)]
        public string NoticePreview { get; set; }

        public DateTime? PublishedDate { get; set; }

        [Required]
        public string NoticeContent { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
